using NMocker.Extentions;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly int line;
        private readonly string file;

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
            try
            {
                StackFrame stackFrame = new StackTrace(true).GetFrames()[4];
                file = stackFrame.GetFileName();
                line = stackFrame.GetFileLineNumber();
            }
            catch
            {
                file = string.Empty;
                line = -1;
            }
        }

        private string Dump(InvocationMatcher invocationMatcher)
        {
            return (invocationMatcher.Matches(this) ? "--->" : "    ") + method.Dump(args);
        }

        public string Dump()
        {
            return method.Dump(args) + string.Format(" called at {0}:{1}", file, line);
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

        internal static int Matched(InvocationMatcher invocationMatcher)
        {
            return invocations.Count(i => invocationMatcher.Matches(i));
        }

        internal static string DumpAll(InvocationMatcher invocationMatcher)
        {
            return invocations.Aggregate(new StringBuilder(),
                (builder, invocation) => builder.Append(invocation.Dump(invocationMatcher)).Append('\n')).ToString();
        }
    }
}
