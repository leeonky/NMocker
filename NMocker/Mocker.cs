using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NMocker
{
    public class Mocker
    {
        private readonly InvocationMatcher invocationMatcher;
        private Then then;

        private Mocker(InvocationMatcher invocationMatcher)
        {
            this.invocationMatcher = invocationMatcher;
        }

        private readonly static List<Mocker> mockers = new List<Mocker>();

        private void ThenReturn(Then then)
        {
            invocationMatcher.PatchMethod(new HarmonyMethod(GetType().GetMethod("ReturnPrefix")));
            mockers.Add(this);
            this.then = then;
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, object[] __args, ref object __result)
        {
            Invocation invocation = Invocation.ActualCall(__originalMethod, __args);
            Mocker mocker = mockers.LastOrDefault(m => m.invocationMatcher.Matches(invocation));
            if (mocker != null)
            {
                mocker.invocationMatcher.ProcessRefAndOutArgs(__args);
                return mocker.then.DoThen(__args, ref __result);
            }
            return true;
        }

        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(new InvocationMatcher(action));
        }

        public static Mocker When(Type type, string method, params object[] args)
        {
            return new Mocker(new InvocationMatcher(type, method, args == null ? new object[] { null } : args));
        }

        public void ThenReturn(object value)
        {
            ThenReturn(new ThenValue(value));
        }

        public void Then(Func<object[], object> then)
        {
            ThenReturn(new ThenLambda(then));
        }

        public void ThenCallActual()
        {
            ThenReturn(new ThenActual());
        }

        public void ThenDefault()
        {
            ThenReturn(new Then());
        }

        public static void Clear()
        {
            mockers.Clear();
            InvocationMatcher.Clear();
            Invocation.Clear();
        }
    }
}
