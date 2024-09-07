using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace nmocker
{
    public class When
    {
        private readonly MethodInfo methodInfo;
        private List<Predicate<object>> arguments = new List<Predicate<object>>();

        public When(Expression<Action> action)
        {
            this.methodInfo = SymbolExtensions.GetMethodInfo(action);
            if (action.Body is MethodCallExpression methodCallExpression)
            {
                foreach (var argument in methodCallExpression.Arguments)
                {
                    arguments.Add(actual =>
                    NewMethod(actual, argument));
                }
            }
        }

        private static bool NewMethod(object actual, Expression argument)
        {
            return Object.Equals(actual, ((ConstantExpression)argument).Value);
        }

        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        public bool Matches(MethodBase method, object[] __args)
        {
            return methodInfo == method && allArgMatches(__args);
        }

        private bool allArgMatches(object[] args)
        {
            if (args.Length != arguments.Count)
                return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (!arguments[i].Invoke(args[i]))
                    return false;
            }
            return true;
        }
    }

    public class Mocker
    {
        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(new When(action));
        }

        private When when;
        private Mocker(When when)
        {
            this.when = when;
        }

        private static Harmony harmony = new Harmony("Mocker");
        private static List<When> whens = new List<When>();
        private static List<Then> thens = new List<Then>();

        public void ThenReturn(object value)
        {
            HarmonyMethod prefix = new HarmonyMethod(typeof(Mocker).GetMethod("ReturnPrefix"));
            harmony.Patch(when.MethodInfo, prefix);
            whens.Add(when);
            thens.Add(new Then(value));
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, object[] __args, ref object __result)
        {
            for (int i = 0; i < whens.Count; i++)
            {
                if (whens[i].Matches(__originalMethod, __args))
                    return thens[i].doThen(__args, ref __result);
            }
            return false;
        }

        public static void clear()
        {
            harmony.UnpatchAll();
            whens.Clear();
            thens.Clear();
        }
    }

    public class Then
    {
        private object value;

        public Then(object value)
        {
            this.value = value;
        }

        public bool doThen(object[] args, ref object result)
        {
            result = value;
            return false;
        }
    }
}
