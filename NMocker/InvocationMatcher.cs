﻿using HarmonyLib;
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
        private List<Predicate<object>> arguments;

        public MethodInfo Method
        {
            get
            {
                return methodInfo;
            }
        }

        public InvocationMatcher(MethodInfo methodInfo, List<Predicate<object>> arguments)
        {
            this.methodInfo = methodInfo;
            this.arguments = new List<Predicate<object>>(arguments);
        }

        public bool Matches(Invocation invocation)
        {
            if (methodInfo != invocation.Method || invocation.Arguments.Length != arguments.Count)
                return false;
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                if (!arguments[i].Invoke(invocation.Arguments[i]))
                    return false;
            }
            return true;
        }

        public static InvocationMatcher Create(Expression<Action> action)
        {
            if (action.Body is MethodCallExpression methodCallExpression)
            {
                List<Predicate<object>> arguments = new List<Predicate<object>>();
                foreach (var argument in methodCallExpression.Arguments)
                {
                    arguments.Add(ToPredicate(argument));
                }
                return new InvocationMatcher(SymbolExtensions.GetMethodInfo(action), arguments);
            }
            throw new InvalidOperationException();
        }

        public static InvocationMatcher Create(Type type, string method, params IArg[] args)
        {
            Func<MethodInfo, bool> targetMethod = m => m.IsStatic && m.Name == method
                && m.GetParameters().Select(p => p.ParameterType).ToArray().SequenceEqual(args.Select(a => a.Type).ToArray());
            return new InvocationMatcher(type.GetDeclaredMethods().First(targetMethod), args.Select(a => a.Predicate).ToList());
        }

        private static Predicate<object> ToPredicate(Expression argument)
        {
            if (argument is ConstantExpression)
                return actual => Object.Equals(actual, ((ConstantExpression)argument).Value);
            if (argument is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Operand is MethodCallExpression methodCall
                    && methodCall.Method.DeclaringType.IsGenericType
                    && methodCall.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Arg<>))
                {
                    IArg a = (IArg)Expression.Lambda(methodCall).Compile().DynamicInvoke();
                    return a.Predicate;
                }
            }
            throw new InvalidOperationException();
        }
    }
}