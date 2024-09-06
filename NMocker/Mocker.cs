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

        public When(Expression<Action> action)
        {
            this.methodInfo = SymbolExtensions.GetMethodInfo(action);
        }

        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
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
        private static StaticMockerAction staticMockerAction = new StaticMockerAction();

        public void ThenReturn(object value)
        {
            HarmonyMethod prefix = new HarmonyMethod(typeof(Mocker).GetMethod("ReturnPrefix"));
            harmony.Patch(when.MethodInfo, prefix);

            staticMockerAction.Add(when.MethodInfo, value);
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, ref object[] __args, ref object __result)
        {
            __result = staticMockerAction.getResult(__originalMethod);
            return false;
        }
    }

    public class StaticMockerAction
    {
        private IDictionary<MethodBase, object> actions = new Dictionary<MethodBase, object>();

        public void Add(MethodInfo method, object result)
        {
            actions.Add(method, result);
        }

        public object getResult(MethodBase method)
        {
            return actions[method];
        }
    }
}
