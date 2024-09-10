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
                    arguments.Add(argMatcher(argument));
                }
            }
        }

        private static Predicate<object> argMatcher(Expression argument)
        {
            if (argument is ConstantExpression)
                return actual => Object.Equals(actual, ((ConstantExpression)argument).Value);
            if (argument is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Operand is MethodCallExpression methodCall
                    && methodCall.Method.DeclaringType.IsGenericType
                    && methodCall.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Arg<>))
                {
                    IArg a = (IArg)Expression.Lambda(methodCall).Compile().DynamicInvoke();
                    return actual => a.Matches(actual);
                }
            }
            throw new InvalidOperationException();
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
            return then.DoThen(args, ref result);
        }

        private static Harmony harmony = new Harmony("Mocker");
        private static List<Mocker> mockers = new List<Mocker>();
        private static HashSet<MethodInfo> patches = new HashSet<MethodInfo>();

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
            if (mocker != null)
                return mocker.Then(__args, ref __result);
            return false;
        }

        public static Mocker When(Expression<Action> action)
        {
            return new Mocker(action);
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


    interface IArg
    {
        bool Matches(object a);
    }

    public class Arg<A> : IArg
    {
        private Predicate<A> matcher;

        public Arg(Predicate<A> matcher)
        {
            this.matcher = matcher;
        }

        public static Arg<A> Any()
        {
            return new Arg<A>(a => true);
        }

        public static Arg<A> That(Predicate<A> matcher)
        {
            return new Arg<A>(matcher);
        }

        public static implicit operator A(Arg<A> arg)
        {
            return default(A);
        }

        public bool Matches(object a)
        {
            return matcher.Invoke((A)a);
        }
    }
}
