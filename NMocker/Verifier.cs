using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace NMocker
{
    public class Verifier
    {
        public static VerificationGroup Times(int times)
        {
            return new VerificationGroup(times == 0 ? "no call" : string.Format("to call {0} times", times), times.Equals);
        }

        public static VerificationGroup AtLeast(int times)
        {
            return new VerificationGroup(string.Format("to call at least {0} times", times), i => i >= times);
        }

        public static VerificationGroup AtMost(int times)
        {
            return new VerificationGroup(string.Format("to call at most {0} times", times), i => i <= times);
        }
        public static VerificationGroup Call(Expression<Action> invocation)
        {
            return AtLeast(1).Call(invocation, 1);
        }

        public static VerificationGroup Never { get { return Times(0); } }

        public static VerificationGroup Once { get { return Times(1); } }
    }

    public class Verification
    {
        public readonly int line;
        public readonly string file;
        public readonly InvocationMatcher invocationMatcher;

        public Verification(InvocationMatcher invocationMatcher, int depth)
        {
            StackFrame stackFrame = new StackTrace(true).GetFrame(depth + 2);
            this.file = stackFrame.GetFileName();
            this.line = stackFrame.GetFileLineNumber();
            this.invocationMatcher = invocationMatcher;
        }
    }

    public class VerificationGroup
    {
        private Predicate<int> testTimes;
        private Verification verification;
        private string expectationMessage;

        public VerificationGroup(string message, Predicate<int> testTimes)
        {
            this.expectationMessage = message;
            this.testTimes = testTimes;
        }

        public VerificationGroup Call(Expression<Action> invocation, int depth = 0)
        {
            this.verification = new Verification(new InvocationMatcher(invocation), depth);
            return this;
        }

        public void Verify()
        {
            int matched = Invocation.Matched(verification.invocationMatcher);
            if (!testTimes.Invoke(matched))
            {
                StringBuilder message = new StringBuilder();
                message.Append(string.Format("Unsatisfied invocation verification at {0}:{1}", verification.file, verification.line));
                message.Append("\nAll invocations:\n");
                message.Append(Invocation.DumpAll(verification.invocationMatcher));
                message.Append(string.Format("Expected {0}, but actually call {1} times.", expectationMessage, matched));
                throw new UnexpectedCallException(message.ToString());
            }
        }
    }
}
