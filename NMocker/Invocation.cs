using NMocker.Extentions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NMocker
{
    public class Invocation
    {
        private readonly MethodBase method;
        private readonly object instance;
        private readonly object[] args;
        private readonly StackFrame stackFrame;

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
            stackFrame = new StackTrace(true).GetFrames()[5];
        }

        public string Dump()
        {
            return method.Dump(args) + string.Format(" called at {0}", stackFrame.PositionString());
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
    }
}
