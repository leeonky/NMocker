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

        public MethodInfo Method
        {
            get
            {
                return methodInfo;
            }
        }

        public InvocationMatcher(MethodInfo methodInfo, IEnumerable<ArgMatcher> argMatchers)
        {
            this.methodInfo = methodInfo;
            this.argMatchers = new List<ArgMatcher>(argMatchers);
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

        public static InvocationMatcher Create(Expression<Action> action)
        {
            if (action.Body is MethodCallExpression method)
                return new InvocationMatcher(SymbolExtensions.GetMethodInfo(action), method.Arguments.Select(CompileArgMatcher));
            throw new InvalidOperationException();
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

        public static InvocationMatcher Create(Type type, string methodName, params object[] args)
        {
            IEnumerable<ArgMatcher> argMatchers = args.Select(CastToMatcher);
            MethodInfo method = type.GetDeclaredMethods().First(m => m.IsStatic && m.Name == methodName
                && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(argMatchers.Select(a => a.Type())));
            return new InvocationMatcher(method, argMatchers);
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
    }
}
