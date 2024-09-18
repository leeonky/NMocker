using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private readonly static Harmony harmony = new Harmony("Mocker");
        private readonly static List<Mocker> mockers = new List<Mocker>();
        private readonly static HashSet<MethodInfo> patches = new HashSet<MethodInfo>();

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
            {
                mocker.invocationMatcher.ProcessRefAndOutArgs(__args);
                return mocker.then.DoThen(__args, ref __result);
            }
            return true;
        }

        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(InvocationMatcher.Create(action));
        }

        public static Mocker When(Type type, string method, params object[] args)
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

        public void ThenDefault()
        {
            ThenReturn(new Then());
        }

        public static void Clear()
        {
            harmony.UnpatchAll();
            mockers.Clear();
            patches.Clear();
            Invocation.Clear();
        }
    }

    public class Then
    {
        public virtual bool DoThen(object[] args, ref object result)
        {
            return false;
        }
    }

    public class ThenValue : Then
    {
        private readonly object value;

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
        private readonly Func<object[], object> then;

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

    public abstract class ArgMatcher
    {
        public abstract Type Type();

        public virtual void ProcessRef(ref object arg)
        {
        }

        public abstract bool Matches(object arg);
    }

    public class RawTypeArgMatcher : ArgMatcher
    {
        private readonly Type type;
        private readonly Predicate<object> predicate;

        public RawTypeArgMatcher(Type type, Predicate<object> predicate)
        {
            this.type = type;
            this.predicate = predicate;
        }

        public override bool Matches(object arg)
        {
            return predicate.Invoke(arg);
        }

        public override Type Type()
        {
            return type;
        }
    }

    public class GenericArgMatcher<A> : ArgMatcher
    {
        private readonly Predicate<A> matcher;

        public GenericArgMatcher(Predicate<A> matcher)
        {
            this.matcher = matcher;
        }

        public static implicit operator A(GenericArgMatcher<A> _)
        {
            return default(A);
        }

        public override bool Matches(object arg)
        {
            return matcher.Invoke((A)arg);
        }

        public override Type Type()
        {
            return typeof(A);
        }

        public RefArgMatcher<A> Ref()
        {
            return new RefArgMatcher<A>(matcher);
        }

        public GenericArgMatcher<A> Ref(A value)
        {
            return new RefWithValueArgMatcher<A>(matcher, value);
        }
    }

    public class RefArgMatcher<A> : GenericArgMatcher<A>
    {
        public RefArgMatcher(Predicate<A> matcher) : base(matcher)
        {
        }

        public override Type Type()
        {
            return base.Type().MakeByRefType();
        }
    }

    public class RefWithValueArgMatcher<A> : RefArgMatcher<A>
    {
        private readonly A value;

        public RefWithValueArgMatcher(Predicate<A> matcher, A value) : base(matcher)
        {
            this.value = value;
        }

        public override void ProcessRef(ref object arg)
        {
            arg = value;
        }
    }

    public class Arg
    {
        public static GenericArgMatcher<A> Any<A>()
        {
            return That<A>(a => true);
        }

        public static GenericArgMatcher<A> Is<A>(A value)
        {
            return That<A>(a => object.Equals(a, value));
        }

        public static GenericArgMatcher<A> That<A>(Predicate<A> matcher)
        {
            return new GenericArgMatcher<A>(matcher);
        }

        public static GenericArgMatcher<A> Out<A>()
        {
            return Any<A>().Ref();
        }
    }
}
