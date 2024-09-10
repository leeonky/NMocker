using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace nmocker
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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (method.IsStatic)
                builder.Append("static ");

            builder.Append(method.DeclaringType.Name)
                .Append("::")
                .Append(method.Name)
                .Append('(')
                .Append(string.Join(", ", method.GetParameters().Select((p, index) => p.ParameterType.Name + "<" + args[index] + ">").ToArray()))
                .Append(')');

            return builder.ToString();
        }

        internal static Invocation ActualCall(MethodBase method, object instance, object[] args)
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
            return Invocation.invocations.Count(i => invocationMatcher.Matches(i));
        }

        internal static string Dump(InvocationMatcher invocationMatcher)
        {
            StringBuilder message = new StringBuilder();
            foreach (Invocation invocation in Invocation.invocations)
            {
                if (invocationMatcher.Matches(invocation))
                    message.Append("--->");
                else
                    message.Append("    ");
                message.Append(invocation.ToString()).Append('\n');
            }
            return message.ToString();
        }
    }
}
