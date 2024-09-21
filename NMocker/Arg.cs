using System;

namespace NMocker
{
    public abstract class ArgMatcher
    {
        public virtual void ProcessRef(ref object arg)
        {
        }

        public abstract bool Matches(object arg);

        public abstract bool TypeMatches(Type t);
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

        public override bool TypeMatches(Type t)
        {
            return t == type;
        }
    }

    public class RawValueArgMatcher : ArgMatcher
    {
        private readonly object value;
        public RawValueArgMatcher(object value)
        {
            this.value = value;
        }

        public override bool Matches(object arg)
        {
            return object.Equals(value, arg);
        }

        public override bool TypeMatches(Type t)
        {
            if (value == null)
                return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
            return t == value.GetType();
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

        protected virtual Type Type()
        {
            return typeof(A);
        }

        public override bool TypeMatches(Type t)
        {
            return Type() == t;
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

        protected override Type Type()
        {
            return typeof(A).MakeByRefType();
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

        public static GenericArgMatcher<A> Out<A>(A value)
        {
            return Any<A>().Ref(value);
        }
    }
}
