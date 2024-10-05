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

            public string UnsatisfiedMessage()
            {
                return string.Format("Unsatisfied invocation at {0}", position);
            }

            public bool Hit(Invocation invocation)
            {
                bool hit = invocationMatcher.Matches(invocation);
                if (hit)
                    invocations.Add(invocation);
                return hit;
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
            private VerificationGroup verificationGroup;
            private readonly VerificationGroup root;
            private VerificationGroup next;

            public VerificationGroup(int times)
            {
                this.times = times;
                this.root = this;
            }

            public VerificationGroup(VerificationGroup verificationGroup, int times)
            {
                this.verificationGroup = verificationGroup;
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
                List<HandledInvocation> handeldInvocations = new List<HandledInvocation>();
                root.HitAll(handeldInvocations, new Queue<Invocation>(Invocation.invocations), 0);
                return handeldInvocations;
            }

            private void HitAll(List<HandledInvocation> handeldInvocations, Queue<Invocation> invocations, int v)
            {
                if (invocations.Any())
                {
                    Invocation invocation = invocations.Dequeue();
                    if (!HitCurrentOrNextGroup(handeldInvocations, invocation, invocations, v))
                        handeldInvocations.Add(new HandledInvocation(invocation));
                    next?.HitAll(handeldInvocations, invocations, 0);
                }
            }

            private bool Hit(Verification verification, Invocation invocation, List<HandledInvocation> handeldInvocations)
            {
                if (verification.Hit(invocation))
                {
                    handeldInvocations.Add(new HandledInvocation(invocation, verification));
                    return true;
                }
                return false;
            }

            private bool HitCurrentOrNextGroup(List<HandledInvocation> handeldInvocations, Invocation invocation, Queue<Invocation> invocations, int v)
            {
                if (verifications.Count > v && Hit(verifications[v++], invocation, handeldInvocations))
                {
                    HitAll(handeldInvocations, invocations, v);
                    return true;
                }
                return next?.HitCurrentOrNextGroup(handeldInvocations, invocation, invocations, 0) == true;
            }

            public VerificationGroup Times(int value)
            {
                return new VerificationGroup(this, value);
            }
        }

                //foreach (VerificationResult verificationResult in verificationResults)
                //{
                //    if (times != verificationResult.Hit)
                //    {
                //        StringBuilder message = new StringBuilder();
                //        message.Append(verificationResult.verification.UnsatisfiedMessage());
                //        message.Append("\nAll invocations:\n");
                //        int width = handeldInvocations.Select(r => r.Width).DefaultIfEmpty(0).Max();
                //        foreach (HandledInvocation handledInvocation in handeldInvocations)
                //            message.Append("    ").Append(handledInvocation.Dump(width)).Append('\n');
                //        message.Append(string.Format("Expected to call {0} times, but actually call {1} times.", times, verificationResult.Hit));
                //        throw new UnexpectedCallException(message.ToString());
                //    }
                //}

        public class HandledInvocation
        {

            public HandledInvocation(Invocation invocation, Verification verificationResult = null)
            {
                this.verification = verificationResult;
                Invocation = invocation;
            }

            public int Width
            {
                get {
                    if (verification != null)
                        return verification.MessageHitFrom(verification.invocations.IndexOf(Invocation) + 1).Length;
                    return 0;
                }
            }

            public readonly Invocation Invocation;
            public readonly Verification verification;

            public string Dump(int width)
            {
                if(verification!=null)
                {
                    return verification.MessageHitFrom(verification.invocations.IndexOf(Invocation) + 1) + Invocation.Dump();
                }
                return new String(' ',width) +Invocation.Dump();
            }
        }
    }
}
