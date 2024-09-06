using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace nmocker
{
    public class Mocker
    {
        public static StaticMocker When(Type type, string methodName)
        {
            return new StaticMocker(type, methodName);
        }

        public class StaticMocker
        {
            private Type type;
            private string methodName;

            public StaticMocker(Type type, string methodName)
            {
                this.type = type;
                this.methodName = methodName;
            }
            private static Harmony harmony = new Harmony("Mocker");
            private static StaticMockerAction staticMockerAction = new StaticMockerAction();

            public void ThenReturn(object value)
            {
                HarmonyMethod prefix = new HarmonyMethod(typeof(StaticMocker).GetMethod("ReturnPrefix"));
                MethodInfo methodInfo = type.GetMethod(methodName);
                staticMockerAction.Add(methodInfo, value);
                harmony.Patch(methodInfo, prefix);
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
}
