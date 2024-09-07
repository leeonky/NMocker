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
                    arguments.Add(actual => argMatcher(actual, argument));
                }
            }
        }

        private static bool argMatcher(object actual, Expression argument)
        {
            return Object.Equals(actual, ((ConstantExpression)argument).Value);
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
            return then.doThen(args, ref result);
        }

        private static Harmony harmony = new Harmony("Mocker");
        private static List<Mocker> mockers = new List<Mocker>();
        private static HashSet<MethodInfo> patches = new HashSet<MethodInfo>();

        public void ThenReturn(object value)
        {
            ThenReturn(new Then(value));
        }

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
            if( mocker!=null)
                    return mocker.Then(__args, ref __result);
            return false;
        }

        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(action);
        }

        public static void clear()
        {
            harmony.UnpatchAll();
            mockers.Clear();
            patches.Clear();
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
