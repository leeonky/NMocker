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
        private readonly List<IArgMatcher> argMatchers;

        public MethodInfo Method
        {
            get
            {
                return methodInfo;
            }
        }

        public InvocationMatcher(MethodInfo methodInfo, List<IArgMatcher> argMatchers)
        {
            this.methodInfo = methodInfo;
            this.argMatchers = new List<IArgMatcher>(argMatchers);
        }

        public bool Matches(Invocation invocation)
        {
            if (methodInfo != invocation.Method || invocation.Arguments.Length != argMatchers.Count)
                return false;
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                if (!argMatchers[i].Predicate.Invoke(invocation.Arguments[i]))
                    return false;
            }
            return true;
        }

        public static InvocationMatcher Create(Expression<Action> action)
        {
            if (action.Body is MethodCallExpression methodCallExpression)
                return new InvocationMatcher(SymbolExtensions.GetMethodInfo(action), 
                    methodCallExpression.Arguments.Select(a => CompileArgMatcher(a)).ToList());
            throw new InvalidOperationException();
        }

        public static InvocationMatcher Create(Type type, string method, params object[] args)
        {
            List<IArgMatcher> argMatchers = args.Select(a => castToMatcher(a)).ToList();
            Func<MethodInfo, bool> targetMethod = m => m.IsStatic && m.Name == method
                && m.GetParameters().Select(p => p.ParameterType).ToArray().SequenceEqual(argMatchers.Select(a => a.Type).ToArray());
            return new InvocationMatcher(type.GetDeclaredMethods().First(targetMethod), argMatchers);
        }
        class RawTypeArgMatcher : IArgMatcher
        {
            private readonly Type type;
            private readonly Predicate<object> predicate;

            public RawTypeArgMatcher(Type type, Predicate<object> predicate)
            {
                this.type = type;
                this.predicate = predicate;
            }

            public Predicate<object> Predicate
            {
                get
                {
                    return predicate;
                }
            }

            public Type Type
            {
                get
                {
                    return type;
                }
            }

            public void processRef(ref object arg)
            {
            }
        }

        private static IArgMatcher castToMatcher(object arg)
        {
            if (arg is IArgMatcher iArg)
                return iArg;
            return new RawTypeArgMatcher(arg.GetType(), obj=>object.Equals(obj, arg));
        }

        private static Type GuessType(object arg)
        {
            if (arg is IArgMatcher iArg)
                return iArg.Type;
            return arg.GetType();
        }
        private static Predicate<object> GuessPredicate(object arg)
        {
            if (arg is IArgMatcher iArg)
                return iArg.Predicate;
            return actual => object.Equals(actual, arg);
        }

        private static Predicate<object> CompilePredicate(Expression argument)
        {
            if (argument is ConstantExpression)
                return actual => Object.Equals(actual, ((ConstantExpression)argument).Value);
            if (argument is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Operand is MethodCallExpression methodCall
                    && methodCall.Method.DeclaringType == typeof(Arg))
                return ((IArgMatcher)Expression.Lambda(methodCall).Compile().DynamicInvoke()).Predicate;
            throw new InvalidOperationException();
        }

        private static IArgMatcher CompileArgMatcher(Expression argument)
        {
            if (argument is ConstantExpression)
                return new RawTypeArgMatcher(argument.Type, actual => Object.Equals(actual, ((ConstantExpression)argument).Value));
            if (argument is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Operand is MethodCallExpression methodCall
                    && methodCall.Method.DeclaringType == typeof(Arg))
                return new RawTypeArgMatcher(argument.Type, ((IArgMatcher)Expression.Lambda(methodCall).Compile().DynamicInvoke()).Predicate);
            throw new InvalidOperationException();
        }

        public void ProcessRefAndOutArgs(object[] args)
        {
            for(int index = 0; index < args.Length; index++)
            {
                argMatchers[index].processRef(ref args[index]);
            }
        }
    }
}
