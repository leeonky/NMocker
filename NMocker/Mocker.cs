﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace nmocker
{
    public class Mocker
    {
        private readonly InvocationMatcher invocationMatcher;
        private Then then;

        private Mocker(InvocationMatcher invocationMatcher)
        {
            this.invocationMatcher = invocationMatcher;
        }

        private static Harmony harmony = new Harmony("Mocker");
        private static List<Mocker> mockers = new List<Mocker>();
        private static HashSet<MethodInfo> patches = new HashSet<MethodInfo>();

        private void ThenReturn(Then then)
        {
            if (!patches.Contains(invocationMatcher.Method))
            {
                harmony.Patch(invocationMatcher.Method, new HarmonyMethod(GetType().GetMethod("ReturnPrefix")));
                patches.Add(invocationMatcher.Method);
            }
            mockers.Add(this);
            this.then = then;
        }

        public static bool ReturnPrefix(MethodBase __originalMethod, object[] __args, ref object __result)
        {
            Invocation invocation = Invocation.ActualCall(__originalMethod, null, __args);
            Mocker mocker = mockers.LastOrDefault(m => m.invocationMatcher.Matches(invocation));
            if (mocker != null)
                return mocker.then.DoThen(__args, ref __result);
            return false;
        }

        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(InvocationMatcher.Create(action));
        }

        public static Mocker When(Type type, string method, params IArg[] args)
        {
            return new Mocker(InvocationMatcher.Create(type, method, args));
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

        public static void Clear()
        {
            harmony.UnpatchAll();
            mockers.Clear();
            patches.Clear();
            Invocation.Clear();
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

    public class ThenActual : Then
    {
        public override bool DoThen(object[] args, ref object result)
        {
            return true;
        }
    }

    public interface IArg
    {
        Predicate<object> Predicate
        {
            get;
        }

        Type Type
        {
            get;
        }
    }

    public class Arg<A> : IArg
    {
        private Predicate<object> matcher;

        public Arg(Predicate<A> matcher)
        {
            this.matcher = actual => matcher.Invoke((A)actual);
        }

        public static Arg<A> Any()
        {
            return That(a => true);
        }

        public static Arg<A> That(Predicate<A> matcher)
        {
            return new Arg<A>(matcher);
        }

        public static implicit operator A(Arg<A> arg)
        {
            return default(A);
        }

        public Predicate<object> Predicate
        {
            get
            {
                return matcher;
            }
        }

        public Type Type
        {
            get { return typeof(A); }
        }
    }
}
