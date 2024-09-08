using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace nmocker
{
    public class Mocker
    {
        private readonly MethodInfo methodInfo;
        private List<Predicate<object>> arguments = new List<Predicate<object>>();
        private Then then;

        private Mocker(Expression<Action> action)
        {
            this.methodInfo = SymbolExtensions.GetMethodInfo(action);
            if (action.Body is MethodCallExpression methodCallExpression)
            {
                foreach (var argument in methodCallExpression.Arguments)
                {
                    arguments.Add(argMatcher(argument));
                }
            }
        }

        private static Predicate<object> argMatcher(Expression argument)
        {
            if (argument is ConstantExpression)
                return actual => Object.Equals(actual, ((ConstantExpression)argument).Value);
            if (argument is MethodCallExpression methodCall)
            {
                if (methodCall.Method.IsGenericMethod && methodCall.Method.GetGenericMethodDefinition() == typeof(Arg).GetMethod("Any"))
                {
                    return actual => true;
                }
                if (methodCall.Method.IsGenericMethod && methodCall.Method.GetGenericMethodDefinition() == typeof(Arg).GetMethod("That"))
                {
                    Delegate lambda = ((LambdaExpression)methodCall.Arguments[0]).Compile();
                    return actual => (bool)lambda.DynamicInvoke(actual);
                }
            }
            throw new InvalidOperationException();
        }

        public bool Matches(MethodBase method, object[] args)
        {
            if (methodInfo != method || args.Length != arguments.Count)
                return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (!arguments[i].Invoke(args[i]))
                    return false;
            }
            return true;
        }

        public bool Then(object[] args, ref object result)
        {
            return then.DoThen(args, ref result);
        }

        private static Harmony harmony = new Harmony("Mocker");
        private static List<Mocker> mockers = new List<Mocker>();
        private static HashSet<MethodInfo> patches = new HashSet<MethodInfo>();

        private void ThenReturn(Then then)
        {
            if (!patches.Contains(methodInfo))
            {
                harmony.Patch(methodInfo, new HarmonyMethod(GetType().GetMethod("ReturnPrefix")));
                patches.Add(methodInfo);
            }
            mockers.Add(this);
            this.then = then;
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, object[] __args, ref object __result)
        {
            Mocker mocker = mockers.LastOrDefault(m => m.Matches(__originalMethod, __args));
            if (mocker != null)
                return mocker.Then(__args, ref __result);
            return false;
        }

        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(action);
        }

        public void ThenReturn(object value)
        {
            ThenReturn(new ThenValue(value));
        }
        public void Then(Func<object[], object> then)
        {
            ThenReturn(new ThenLambda(then));
        }

        public static void Clear()
        {
            harmony.UnpatchAll();
            mockers.Clear();
            patches.Clear();
        }
    }

    public abstract class Then
    {
        public abstract bool DoThen(object[] args, ref object result);
    }

    public class ThenValue : Then
    {
        private object value;

        public ThenValue(object value)
        {
            this.value = value;
        }

        public override bool DoThen(object[] args, ref object result)
        {
            result = value;
            return false;
        }
    }

    public class ThenLambda : Then
    {
        private Func<object[], object> then;

        public ThenLambda(Func<object[], object> then)
        {
            this.then = then;
        }

        public override bool DoThen(object[] args, ref object result)
        {
            result = then.Invoke(args);
            return false;
        }
    }


    public class Arg
    {
        public static T Any<T>()
        {
            throw new InvalidOperationException();
        }

        public static T That<T>(Predicate<T> matcher)
        {
            throw new InvalidOperationException();
        }
    }
}
