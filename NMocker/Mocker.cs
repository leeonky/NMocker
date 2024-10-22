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
        private readonly static List<Mocker> mockers = new List<Mocker>();
        private readonly InvocationMatcher invocationMatcher;
        private Then then;

        public Mocker(InvocationMatcher invocationMatcher)
        {
            this.invocationMatcher = invocationMatcher;
        }

        public static void Clear()
        {
            mockers.Clear();
            InvocationMatcher.Clear();
            Invocation.Clear();
        }

        protected static bool Prefix(MethodBase __originalMethod, object[] __args, ref object __result)
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

        public static Mock When(Expression<Action> action)
        {
            return new Mock(InvocationMatcher.Create(action));
        }

        public static Stub When<T>(Expression<Func<T>> action)
        {
            return new Stub(InvocationMatcher.Create(action));
        }

        public static Stub When(Type type, string method, params object[] args)
        {
            return new Stub(InvocationMatcher.Create(type, method, args == null ? new object[] { null } : args));
        }

        public static Mock WhenVoid(Type type, string method, params object[] args)
        {
            return new Mock(InvocationMatcher.Create(type, method, args == null ? new object[] { null } : args));
        }

        public void ThenCallActual()
        {
            Then(new ThenActual());
        }

        public void ThenDefault()
        {
            Then(new Then());
        }

        protected void Then(Then then)
        {
            invocationMatcher.PatchMethod(new HarmonyMethod(GetType().GetMethod("ReturnPrefix")));
            mockers.Add(this);
            this.then = then;
        }

        public static void Mock(Expression<Action> action)
        {
            When(action).ThenDefault();
        }

        public static void Mock<T>(Expression<Func<T>> action)
        {
            When(action).ThenDefault();
        }
    }

    public class Stub : Mocker
    {
        public Stub(InvocationMatcher invocationMatcher) : base(invocationMatcher)
        {
        }

        public void ThenReturn(object value)
        {
            Then(new ThenValue(value));
        }

        public void Then(Func<object[], object> then)
        {
            Then(new ThenLambda(then));
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, object[] __args, ref object __result)
        {
            return Prefix(__originalMethod, __args, ref __result);
        }
    }

    public class Mock : Mocker
    {
        public Mock(InvocationMatcher invocationMatcher) : base(invocationMatcher)
        {
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, object[] __args)
        {
            object placeholder = null;
            return Prefix(__originalMethod, __args, ref placeholder);
        }

        public void Then(Action<object[]> action)
        {
            Then(new ThenLambda(objs =>
            {
                action.Invoke(objs);
                return null;
            }));
        }
    }
}
