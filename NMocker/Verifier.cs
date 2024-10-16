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
            return new VerificationGroup(n => n == times, $"to call {times} times");
        }

        public static VerificationGroup AtLeast(int times)
        {
            return new VerificationGroup(n => n >= times, $"to call at least {times} times");
        }

        public static VerificationGroup Once()
        {
            return Times(1);
        }

        public static VerificationGroup AtMost(int times)
        {
            return new VerificationGroup(n => n <= times, $"to call at most {times} times");
        }

        public static VerificationGroupForVerifierDefault Called(Expression<Action> invocation, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            VerificationGroupForVerifierDefault verificationGroup = new VerificationGroupForVerifierDefault(n => n >= 1, "to call at least 1 times");
            ((VerificationGroup)verificationGroup).Called(invocation, line, file);
            return verificationGroup;
        }

        public class Verification
        {
            private readonly InvocationMatcher invocationMatcher;
            private readonly string position;
            private readonly List<Invocation> invocations = new List<Invocation>();

            public Verification(InvocationMatcher invocationMatcher, string position)
            {
                this.invocationMatcher = invocationMatcher;
                this.position = position;
            }

            public string MessageHitFrom(Invocation invocation)
            {
                return string.Format("hit({0}) from {1} => ", invocations.IndexOf(invocation) + 1, position);
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
            private List<Verification> verifications = new List<Verification>();
            private readonly VerificationGroup root;
            private VerificationGroup next;
            private readonly Predicate<int> checking;
            private readonly string message;

            public VerificationGroup(Predicate<int> checking, string message, VerificationGroup root = null)
            {
                this.checking = checking;
                this.message = message;
                if (root == null)
                {
                    this.root = this;
                }
                else
                {
                    this.root = root.root;
                    root.next = this;
                }
            }

            public bool Failed(int times)
            {
                return !checking.Invoke(times);
            }

            public string Message
            {
                get { return message; }
            }

            public VerificationGroup Called(Expression<Action> invocation, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
            {
                this.verifications.Add(new Verification(new InvocationMatcher(invocation), string.Format("{0}:{1}", file, line)));
                return this;
            }

            public void Verify()
            {
                List<HandledInvocation> handledInvocations = new List<HandledInvocation>();
                root.HitAll(handledInvocations, new Queue<Invocation>(Invocation.invocations), 0);
                bool failed = false;
                StringBuilder message = new StringBuilder("Unsatisfied invocation:\n");
                for (VerificationGroup verificationGroup = root; verificationGroup != null; verificationGroup = verificationGroup.next)
                    foreach (Verification verification in verificationGroup.verifications)
                        if (verificationGroup.Failed(verification.HitCount))
                        {
                            failed = true;
                            message.Append($"    Expected {verificationGroup.Message}, but {verification.ActualMessage}\n");
                        }
                if (failed)
                {
                    int width = handledInvocations.Select(r => r.Width).DefaultIfEmpty(0).Max();
                    message.Append("All invocations:");
                    foreach (HandledInvocation handledInvocation in handledInvocations)
                        message.Append("\n    ").Append(handledInvocation.Dump(width));
                    throw new UnexpectedCallException(message.ToString());
                }
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

            public VerificationGroup Times(int times)
            {
                return new VerificationGroup(n => n == times, $"to call {times} times", this);
            }

            public VerificationGroup AtLeast(int times)
            {
                return new VerificationGroup(n => n >= times, $"to call at least {times} times", this);
            }

            public VerificationGroup Once()
            {
                return Times(1);
            }

            public VerificationGroup AtMost(int times)
            {
                return new VerificationGroup(n => n <= times, $"to call at most {times} times", this);
            }
        }

        public class VerificationGroupForVerifierDefault : VerificationGroup
        {
            public VerificationGroupForVerifierDefault(Predicate<int> checking, string message, VerificationGroupForVerifierDefault root = null)
                : base(checking, message, root) { }

            new public VerificationGroupForVerifierDefault Called(Expression<Action> invocation, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
            {
                VerificationGroupForVerifierDefault verificationGroup = new VerificationGroupForVerifierDefault(n => n >= 1, "to call at least 1 times", this);
                ((VerificationGroup)verificationGroup).Called(invocation, line, file);
                return verificationGroup;
            }
        }


        public class HandledInvocation
        {
            public HandledInvocation(Invocation invocation, Verification verification = null)
            {
                this.verification = verification;
                this.invocation = invocation;
            }

            public int Width
            {
                get
                {
                    if (verification != null)
                        return verification.MessageHitFrom(invocation).Length;
                    return 0;
                }
            }

            private readonly Invocation invocation;
            private readonly Verification verification;

            public string Dump(int width)
            {
                if (verification != null)
                {
                    string messageHitFrom = verification.MessageHitFrom(invocation);
                    int sub = width - messageHitFrom.Length;
                    if (sub > 0)
                        messageHitFrom.Replace("=>", new String(' ', sub) + "=>");
                    return messageHitFrom + invocation.Dump();
                }
                return new String(' ', width) + invocation.Dump();
            }
        }
    }
}
