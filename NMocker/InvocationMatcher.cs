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

        public InvocationMatcher(Expression<Action> action)
        {
            if (action.Body is MethodCallExpression method)
            {
                methodInfo = SymbolExtensions.GetMethodInfo(action);
                argMatchers = method.Arguments.Select(CompileArgMatcher).ToList();
                return;
            }
            throw new InvalidOperationException();
        }

        public InvocationMatcher(Type type, string methodName, object[] args)
        {
            argMatchers = args.Select(CastToMatcher).ToList();
            List<MethodInfo> methods = type.GetDeclaredMethods().FindAll(m => m.IsStatic && m.Name == methodName && ArgTypesMatched(m));
            if (!methods.Any())
                throw new ArgumentException("No matching method found");
            if (methods.Count > 1)
                throw new ArgumentException(methods.Aggregate(new StringBuilder("Ambiguous method between the following:"),
                    (builder, method) => builder.Append("\n    ").Append(method.Dump())).ToString());
            methodInfo = methods[0];
        }

        private bool ArgTypesMatched(MethodInfo m)
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
            if (argument is ConstantExpression)
                return new RawTypeArgMatcher(argument.Type, actual => Object.Equals(actual, ((ConstantExpression)argument).Value));
            if (argument is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Operand is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Arg))
                return new RawTypeArgMatcher(argument.Type, ((ArgMatcher)Expression.Lambda(methodCall).Compile().DynamicInvoke()).Matches);
            throw new InvalidOperationException();
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
