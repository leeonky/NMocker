using NMocker.Extentions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NMocker
{
    public class Invocation
    {
        private readonly MethodBase method;
        private readonly object instance;
        private readonly object[] args;

        public MethodBase Method
        {
            get { return method; }
        }

        public object Instance
        {
            get { return instance; }
        }

        public object[] Arguments
        {
            get { return args; }
        }

        public Invocation(MethodBase method, object instance, object[] args)
        {
            this.method = method;
            this.instance = instance;
            this.args = args;
        }

        private string Dump(InvocationMatcher invocationMatcher)
        {
            return (invocationMatcher.Matches(this) ? "--->" : "    ") + method.Dump(args);
        }

        internal static Invocation ActualCall(MethodBase method, object[] args)
        {
            Invocation invocation = new Invocation(method, null, args);
            invocations.Add(invocation);
            return invocation;
        }

        internal static void Clear()
        {
            invocations.Clear();
        }

        internal static List<Invocation> invocations = new List<Invocation>();

        internal static int Matched(InvocationMatcher invocationMatcher, int position = 0)
        {
            return invocations.Skip(position).Count(i => invocationMatcher.Matches(i));
        }

        internal static string DumpAll(InvocationMatcher invocationMatcher)
        {
            return invocations.Aggregate(new StringBuilder(),
                (builder, invocation) => builder.Append(invocation.Dump(invocationMatcher)).Append('\n')).ToString();
        }
    }
}
