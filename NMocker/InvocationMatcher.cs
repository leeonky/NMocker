using HarmonyLib;
using NMocker.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NMocker
{
    public class InvocationMatcher
    {
        private readonly MethodInfo methodInfo;
        private readonly List<ArgMatcher> argMatchers;

        public InvocationMatcher(MethodInfo methodInfo, List<ArgMatcher> argMatchers)
        {
            this.methodInfo = methodInfo;
            this.argMatchers = argMatchers;
        }

        public bool IsVoidMethod { get { return methodInfo.ReturnType == typeof(void); } }

        public static InvocationMatcher Create<T>(Expression<T> action)
        {
            if (action.Body is MethodCallExpression method)
            {
                return new InvocationMatcher(SymbolExtensions.GetMethodInfo(action),
                    method.Arguments.Select(CompileArgMatcher).ToList());
            }
            if (action.Body is MemberExpression member && member.Member is PropertyInfo property)
            {
                return new InvocationMatcher(property.GetMethod, new List<ArgMatcher>());
            }
            throw new InvalidOperationException();
        }

        public static InvocationMatcher Create(Type type, string methodName, object[] args)
        {
            var argMatchers = args.Select(CastToMatcher).ToList();
            List<MethodInfo> methods = FindMethod(type, methodName, argMatchers);
            if (!methods.Any())
                throw new ArgumentException("No matching method found");
            if (methods.Count > 1)
                throw new ArgumentException(methods.Aggregate(new StringBuilder("Ambiguous method between the following:"),
                    (builder, method) => builder.Append("\n    ").Append(method.Dump())).ToString());
            return new InvocationMatcher(methods[0], argMatchers);
        }

        private static List<MethodInfo> FindMethod(Type type, string methodName, List<ArgMatcher> argMatchers)
        {
            return type.GetDeclaredProperties().Where(p => p.CanRead && p.Name == methodName).Select(p => p.GetMethod).Where(m => m.IsStatic)
                .Concat(type.GetDeclaredMethods().Where(m => m.IsStatic && m.Name == methodName && ArgTypesMatched(m, argMatchers))).ToList();
        }

        private static bool ArgTypesMatched(MethodInfo m, List<ArgMatcher> argMatchers)
        {
            ParameterInfo[] parameters = m.GetParameters();
            return parameters.Length == argMatchers.Count && Enumerable.Range(0, parameters.Length)
                .All(i => argMatchers[i].TypeMatches(parameters[i].ParameterType));
        }

        public bool Matches(Invocation invocation)
        {
            return methodInfo == invocation.Method && invocation.Arguments.Length == argMatchers.Count
                && Enumerable.Range(0, invocation.Arguments.Length)
                .All(i => argMatchers[i].Matches(invocation.Arguments[i]));
        }

        private static ArgMatcher CompileArgMatcher(Expression argument)
        {
            ArgMatcher argMatcher;
            if (argument is ConstantExpression)
                argMatcher = new RawTypeArgMatcher(argument.Type, actual => Object.Equals(actual, ((ConstantExpression)argument).Value));
            else if (argument is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
                argMatcher = Resolve(unaryExpression.Operand);
            else
                argMatcher = Resolve(argument);
            if (argMatcher != null)
                return argMatcher;
            return new RawValueArgMatcher(Expression.Lambda(argument).Compile().DynamicInvoke());
        }

        private static ArgMatcher Resolve(Expression argument)
        {
            if (argument is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Arg))
                return new RawTypeArgMatcher(argument.Type, ((ArgMatcher)Expression.Lambda(methodCall).Compile().DynamicInvoke()).Matches);
            return null;
        }

        private static ArgMatcher CastToMatcher(object arg)
        {
            return arg is ArgMatcher matcher ? matcher : new RawValueArgMatcher(arg);
        }

        public void ProcessRefAndOutArgs(object[] args)
        {
            for (int index = 0; index < args.Length; index++)
                argMatchers[index].ProcessRef(ref args[index]);
        }

        private readonly static Harmony harmony = new Harmony("Mocker");
        private readonly static HashSet<MethodInfo> patches = new HashSet<MethodInfo>();

        public static void Clear()
        {
            harmony.UnpatchAll();
            patches.Clear();
        }

        public void PatchMethod(HarmonyMethod prefix)
        {
            if (!patches.Contains(methodInfo))
            {
                harmony.Patch(methodInfo, prefix);
                patches.Add(methodInfo);
            }
        }
    }
}
