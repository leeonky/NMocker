using System;
using System.Collections.Generic;
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
        public int matched = 0;

        public Verification(InvocationMatcher invocationMatcher, int depth)
        {
            StackFrame stackFrame = new StackTrace(true).GetFrame(depth + 2);
            this.file = stackFrame.GetFileName();
            this.line = stackFrame.GetFileLineNumber();
            this.invocationMatcher = invocationMatcher;
        }
    }

    public class HittingInvocation
    {
        public readonly Invocation invocation;
        public readonly Verification verification;
        public readonly int hitting;

        public HittingInvocation(Invocation invocation, Verification verification)
        {
            this.invocation = invocation;
            this.verification = verification;
            this.hitting = verification.matched;
        }
    }

    public class VerificationGroup
    {
        private Predicate<int> testTimes;
        private List<Verification> verifications = new List<Verification>();
        private string expectationMessage;

        public VerificationGroup(string message, Predicate<int> testTimes)
        {
            this.expectationMessage = message;
            this.testTimes = testTimes;
        }

        public VerificationGroup Call(Expression<Action> invocation, int depth = 0)
        {
            this.verifications.Add(new Verification(new InvocationMatcher(invocation), depth));
            return this;
        }

        public void Verify()
        {
            Verification verification = verifications[0]; //1

            List<object> invocations = new List<object>();
            foreach (Invocation invocation in Invocation.invocations)
            {
                if (verification.invocationMatcher.Matches(invocation))
                {
                    verification.matched++;
                    invocations.Add(new HittingInvocation(invocation, verification));
                }
                else
                    invocations.Add(invocation);
            }
            if (!testTimes.Invoke(verification.matched))
            {
                StringBuilder message = new StringBuilder();
                message.Append(string.Format("Unsatisfied invocation at {0}:{1}", verification.file, verification.line));
                message.Append("\nAll invocations:\n");

                foreach(object invocation in invocations)
                {
                    message.Append("    ");
                    if(invocation is HittingInvocation hitting)
                    {
                        message.Append(string.Format("hit({3}) from {0}:{1} => {2}", hitting.verification.file, hitting.verification.line, hitting.invocation.Dump(), hitting.hitting));
                    }else
                    {
                        message.Append(((Invocation)invocation).Dump());
                    }
                    message.Append('\n');
                }
                message.Append(string.Format("Expected {0}, but actually call {1} times.", expectationMessage, verification.matched));
                throw new UnexpectedCallException(message.ToString());
            }
            //int matched = Invocation.Matched(verification.invocationMatcher);
            //if (!testTimes.Invoke(matched))
            //{
            //    StringBuilder message = new StringBuilder();
            //    message.Append(string.Format("Unsatisfied invocation verification at {0}:{1}", verification.file, verification.line));
            //    message.Append("\nAll invocations:\n");
            //    message.Append(Invocation.DumpAll(verification.invocationMatcher));
            //    message.Append(string.Format("Expected {0}, but actually call {1} times.", expectationMessage, matched));
            //    throw new UnexpectedCallException(message.ToString());
            //}
        }
    }
}
