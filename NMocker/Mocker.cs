using HarmonyLib;
using System;

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
            private static object result;

            public void ThenReturn(object value)
            {
                StaticMocker.result = value;
                HarmonyMethod prefix = new HarmonyMethod(typeof(StaticMocker).GetMethod("ReturnPrefix"));
                harmony.Patch(type.GetMethod(methodName), prefix);
            }

            public static bool ReturnPrefix(ref object __result)
            {
                __result = StaticMocker.result;
                return false;
            }
        }
    }
}
