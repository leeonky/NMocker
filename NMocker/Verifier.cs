using NMocker.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace NMocker
{
    public class Verifier
    {
        public static VerificationGroup Times(int times)
        {
            return new VerificationGroup(times);
        }

        //    public static VerificationGroup AtLeast(int times)
        //    {
        //        return new VerificationGroup(string.Format("to call at least {0} times", times), i => i >= times);
        //    }

        //    public static VerificationGroup AtMost(int times)
        //    {
        //        return new VerificationGroup(string.Format("to call at most {0} times", times), i => i <= times);
        //    }
        //    public static VerificationGroup Call(Expression<Action> invocation)
        //    {
        //        return AtLeast(1).Call(invocation, 1);
        //    }

        //    public static VerificationGroup Never { get { return Times(0); } }

        //    public static VerificationGroup Once { get { return Times(1); } }
        //}

        public class Verification
        {
            public readonly InvocationMatcher invocationMatcher;
            private readonly string position;
            public readonly List<Invocation> invocations = new List<Invocation>();

            public Verification(InvocationMatcher invocationMatcher, string position)
            {
                this.invocationMatcher = invocationMatcher;
                this.position = position;
            }

            public string MessageHitFrom(int hit)
            {
                return string.Format("hit({0}) from {1} => ", hit, position);
            }

            public string ActualMessage
            {
                get { return $"actually call {HitCount} times from {position}"; }
            }

            public bool Hit(Invocation invocation)
            {
                return invocationMatcher.Matches(invocation).Then(() => invocations.Add(invocation));
            }

            public int HitCount
            {
                get { return invocations.Count; }
            }
        }

        public class VerificationGroup
        {
            private readonly int times;
            private List<Verification> verifications = new List<Verification>();
            private readonly VerificationGroup root;
            private VerificationGroup next;

            public VerificationGroup(int times)
            {
                this.times = times;
                this.root = this;
            }

            public VerificationGroup(VerificationGroup verificationGroup, int times)
            {
                this.times = times;
                this.root = verificationGroup.root;
                verificationGroup.next = this;
            }

            public VerificationGroup Call(Expression<Action> invocation, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
            {
                this.verifications.Add(new Verification(new InvocationMatcher(invocation), string.Format("{0}:{1}", file, line)));
                return this;
            }

            public List<HandledInvocation> Verify()
            {
                List<HandledInvocation> handledInvocations = new List<HandledInvocation>();
                root.HitAll(handledInvocations, new Queue<Invocation>(Invocation.invocations), 0);
                bool failed = false;
                StringBuilder message = new StringBuilder("Unsatisfied invocation:\n");
                for (VerificationGroup verificationGroup = root; verificationGroup != null; verificationGroup = verificationGroup.next)
                    foreach (Verification verification in verificationGroup.verifications)
                        if (verificationGroup.times != verification.HitCount)
                        {
                            failed = true;
                            message.Append($"    Expected to call {verificationGroup.times} times, but {verification.ActualMessage}\n");
                        }
                if (failed)
                {
                    message.Append("All invocations:");
                    foreach (HandledInvocation handledInvocation in handledInvocations)
                        message.Append("\n    ").Append(handledInvocation.Dump(0));
                    throw new UnexpectedCallException(message.ToString());
                }
                return handledInvocations;
            }

            private void HitAll(List<HandledInvocation> handledInvocations, Queue<Invocation> invocations, int v)
            {
                if (invocations.Any())
                {
                    Invocation invocation = invocations.Dequeue();
                    if (!HitCurrentOrNextGroup(handledInvocations, invocation, invocations, v))
                    {
                        handledInvocations.Add(new HandledInvocation(invocation));
                        HitAll(handledInvocations, invocations, v);
                    }
                }
            }

            private bool HitCurrentOrNextGroup(List<HandledInvocation> handledInvocations, Invocation invocation, Queue<Invocation> invocations, int v)
            {
                if (verifications.Count > v)
                {
                    if (verifications[v].Hit(invocation))
                    {
                        handledInvocations.Add(new HandledInvocation(invocation, verifications[v]));
                        HitAll(handledInvocations, invocations, v + 1);
                        return true;
                    }
                    return next?.HitCurrentOrNextGroup(handledInvocations, invocation, invocations, 0) == true;
                }
                return verifications.Count != 0 && HitCurrentOrNextGroup(handledInvocations, invocation, invocations, 0);
            }

            public VerificationGroup Times(int value)
            {
                return new VerificationGroup(this, value);
            }
        }

        //        int width = handeldInvocations.Select(r => r.Width).DefaultIfEmpty(0).Max();

        public class HandledInvocation
        {

            public HandledInvocation(Invocation invocation, Verification verificationResult = null)
            {
                this.verification = verificationResult;
                Invocation = invocation;
            }

            public int Width
            {
                get
                {
                    if (verification != null)
                        return verification.MessageHitFrom(verification.invocations.IndexOf(Invocation) + 1).Length;
                    return 0;
                }
            }

            public readonly Invocation Invocation;
            public readonly Verification verification;

            public string Dump(int width)
            {
                if (verification != null)
                {
                    return verification.MessageHitFrom(verification.invocations.IndexOf(Invocation) + 1) + Invocation.Dump();
                }
                return new String(' ', width) + Invocation.Dump();
            }
        }
    }
}
