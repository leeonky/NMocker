﻿using System;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;

namespace nmocker
{
    public class Verifier
    {
        private readonly int line;
        private readonly string file;
        private readonly int times;
        private readonly InvocationMatcher invocationMatcher;

        private Verifier(int times, InvocationMatcher invocationMatcher)
        {
            StackFrame stackFrame = new StackTrace(true).GetFrame(2);
            this.file = stackFrame.GetFileName();
            this.line = stackFrame.GetFileLineNumber();
            this.times = times;
            this.invocationMatcher = invocationMatcher;
        }

        public static void NCalls(int times, Expression<Action> calling)
        {
            new Verifier(times, InvocationMatcher.Create(calling)).Verify();
        }

        private void Verify()
        {
            int matched = Invocation.Matched(invocationMatcher);
            if (matched != times)
            {
                StringBuilder message = new StringBuilder();
                message.Append(string.Format("Unsatisfied invocation verification at {0}:{1}", file, line));
                message.Append("\nAll invocations:\n");
                message.Append(Invocation.Dump(invocationMatcher));
                message.Append(string.Format("Expected to call {0} times, but actually call {1} times.", times, matched));
                throw new UnexpectedCallException(message.ToString());
            }
        }

    }
}