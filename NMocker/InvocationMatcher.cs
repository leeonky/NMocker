using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace nmocker
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
            methodInfo = type.GetDeclaredMethods().First(m => m.IsStatic && m.Name == methodName
                && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(argMatchers.Select(a => a.Type())));
        }

        public bool Matches(Invocation invocation)
        {
            if (methodInfo != invocation.Method || invocation.Arguments.Length != argMatchers.Count)
                return false;
            for (int i = 0; i < invocation.Arguments.Length; i++)
                if (!argMatchers[i].Matches(invocation.Arguments[i]))
                    return false;
            return true;
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
            return arg is ArgMatcher matcher ? matcher
                : new RawTypeArgMatcher(arg.GetType(), obj => object.Equals(obj, arg));
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
