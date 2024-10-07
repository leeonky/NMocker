using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestNMocker
{
    public abstract class LineVerification
    {
        public abstract string ToString(int verifyLineOffset, StackFrame stackFrame);
    }

    public class ExpectVerification : LineVerification
    {
        private string expect;
        private int line;

        public ExpectVerification(string expect, int line)
        {
            this.expect = expect;
            this.line = line;
        }

        public override string ToString(int verifyLineOffset, StackFrame stackFrame)
        {
            return $"{expect} from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + verifyLineOffset + line - 1}";
        }
    }

    public class InvocationVerification : LineVerification
    {
        private string method;
        private int line;

        public InvocationVerification(string method, int line)
        {
            this.method = method;
            this.line = line;
        }

        public override string ToString(int verifyLineOffset, StackFrame stackFrame)
        {
            return $"{method} called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + line}";
        }
    }

    public class HitInvocationVerification : InvocationVerification
    {
        private readonly string hit;
        private readonly int line;

        public HitInvocationVerification(string hit, int line, string method, int line2) : base(method, line2)
        {
            this.hit = hit;
            this.line = line;
        }

        public override string ToString(int verifyLineOffset, StackFrame stackFrame)
        {
            return $"{hit} from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + verifyLineOffset + line - 1} => " + base.ToString(verifyLineOffset, stackFrame);
        }
    }

    public class TestBase
    {
        public StackFrame stackFrame;
        public UnexpectedCallException exception;

        public void VerifyMessage(int verifyLineOffset, params LineVerification[] lines)
        {
            string text = $@"Unsatisfied invocation:
{string.Join("\n", lines.Where(l => l is ExpectVerification).Select(line => "    " + line.ToString(verifyLineOffset, stackFrame)))}
All invocations:{string.Join(string.Empty, lines.Where(l => l is InvocationVerification).Select(line => "\n    " + line.ToString(verifyLineOffset, stackFrame)))}";
            Assert.AreEqual(text.ToString(), exception.Message);
        }

        public LineVerification Expected(string expect, int line)
        {
            return new ExpectVerification(expect, line);
        }

        public LineVerification Invocation(string hit, int line, string method, int line2)
        {
            return new HitInvocationVerification(hit, line, method, line2);
        }

        public LineVerification Invocation(string method, int line)
        {
            return new InvocationVerification(method, line);
        }

        public void ExecuteFailed(Action action)
        {
            exception = Assert.ThrowsException<UnexpectedCallException>(action);
        }
    }

    [TestClass]
    public class TestVerifyTimesAndHit : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.When(() => Target.method(Arg.Any<string>())).ThenDefault();
        }

        public class Target
        {
            public static int method(string s)
            {
                return 0;
            }
        }

        [TestMethod]
        public void no_call_and_any_expectation()
        {
            stackFrame = new StackTrace(true).GetFrame(0);

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Call(() => Target.method("a")).Verify();
            });

            VerifyMessage(4,
                Expected("Expected to call 1 times, but actually call 0 times", 1));

            Verifier.Times(0).Call(() => Target.method("a")).Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(0).Call(() => Target.method("a")).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            Verifier.Times(1).Call(() => Target.method("a")).Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_hit_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Call(() => Target.method("b")).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Invocation("static Target::method(String<a>)", 1));

            Verifier.Times(0).Call(() => Target.method("b")).Verify();
        }

        [TestMethod]
        public void excess_group_and_hit_group_a_hit_b_and_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(1).Call(() => Target.method("b"))
                .Times(0).Call(() => Target.method("a"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 2, "static Target::method(String<a>)", 1));

            Verifier
                .Times(0).Call(() => Target.method("b"))
                .Times(1).Call(() => Target.method("a"))
                .Verify();
        }

        [TestMethod]
        public void hit_group_and_excess_group_a_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(0).Call(() => Target.method("a"))
                .Times(1).Call(() => Target.method("b"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            Verifier
                .Times(1).Call(() => Target.method("a"))
                .Times(0).Call(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void miss_group_and_excess_group_a_hit_b_and_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(1).Call(() => Target.method("b"))
                .Times(1).Call(() => Target.method("c"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("static Target::method(String<a>)", 1));

            Verifier
                .Times(0).Call(() => Target.method("b"))
                .Times(0).Call(() => Target.method("c"))
                .Verify();
        }

        [TestMethod]
        public void single_group_excess_call_a_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
            });

            VerifyMessage(18,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1));
        }

        [TestMethod]
        public void single_group_excess_call_a_hit_x_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                .Call(() => Target.method("x"))
                .Call(() => Target.method("a"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("static Target::method(String<a>)", 1));

            Verifier.Times(0)
                .Call(() => Target.method("x"))
                .Call(() => Target.method("a"))
                .Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 2));

            Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_b_hit_a_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("c"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<b>)", 2));

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("c"))
                    .Verify();
            });

            VerifyMessage(20,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<b>)", 2));
        }

        [TestMethod]
        public void single_group_balanced_a_c_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("c");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(8,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<c>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 3));

            Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_a_b_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0).Call(() => Target.method("a"))
                    .Times(0).Call(() => Target.method("b"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 2));

            Verifier
                .Times(1).Call(() => Target.method("a"))
                .Times(1).Call(() => Target.method("b"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_a_b_c_d_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");
            Target.method("c");
            Target.method("d");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("b"))
                    .Times(0)
                    .Call(() => Target.method("c"))
                    .Call(() => Target.method("d"))
                    .Verify();
            });

            VerifyMessage(10,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Expected("Expected to call 0 times, but actually call 1 times", 4),
                Expected("Expected to call 0 times, but actually call 1 times", 5),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "static Target::method(String<b>)", 2),
                Invocation("hit(1)", 4, "static Target::method(String<c>)", 3),
                Invocation("hit(1)", 5, "static Target::method(String<d>)", 4));

            Verifier
                .Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Times(1)
                .Call(() => Target.method("c"))
                .Call(() => Target.method("d"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_each_group_excess_verification_a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("c");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("b"))
                    .Times(0)
                    .Call(() => Target.method("c"))
                    .Call(() => Target.method("d"))
                    .Verify();
            });

            VerifyMessage(8,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 4),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 4, "static Target::method(String<c>)", 2));

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(1)
                    .Call(() => Target.method("a"))
                    .Call(() => Target.method("b"))
                    .Times(1)
                    .Call(() => Target.method("c"))
                    .Call(() => Target.method("d"))
                    .Verify();
            });

            VerifyMessage(26,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Expected("Expected to call 1 times, but actually call 0 times", 5),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(1)", 4, "static Target::method(String<c>)", 2));
        }

        [TestMethod]
        public void two_group_balanced_each_group_excess_call_a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");
            Target.method("c");
            Target.method("d");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0).Call(() => Target.method("a"))
                    .Times(0).Call(() => Target.method("c"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("static Target::method(String<b>)", 2),
                Invocation("hit(1)", 2, "static Target::method(String<c>)", 3),
                Invocation("static Target::method(String<d>)", 4));

            Verifier
                .Times(1).Call(() => Target.method("a"))
                .Times(1).Call(() => Target.method("c"))
                .Verify();
        }

        [TestMethod]
        public void hit_more_than_once_a_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Call(() => Target.method("a")).Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Invocation("hit(1)", 1, "static Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "static Target::method(String<a>)", 2));

            Verifier.Times(2).Call(() => Target.method("a")).Verify();
        }

    }

    [TestClass]
    public class TestHit
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.When(() => Target.method(Arg.Any<string>())).ThenDefault();
        }

        public class Target
        {
            public static int method(string s)
            {
                return 0;
            }
        }

        StackFrame stackFrame;

        private void AssertInvocation(Verifier.HandledInvocation handledInvocation, string method, int line)
        {
            Assert.AreEqual(string.Format(method + " called at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + line), handledInvocation.Dump(0));
        }

        private void AssertInvocation(Verifier.HandledInvocation handledInvocation, string hit, int hitLine, string method, int line)
        {
            string calledAt = string.Format(method + " called at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + line);
            string hitFrom = string.Format("{0} from {1}:{2} => ", hit, stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + hitLine);
            Assert.AreEqual(hitFrom + calledAt, handledInvocation.Dump(0));
        }

        [TestMethod]
        public void no_call_and_no_expectation()
        {
            Assert.AreEqual(0, Verifier.Times(0).Verify().Count);
        }

        [TestMethod]
        public void one_call_and_no_expectation()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method(string.Empty);

            List<Verifier.HandledInvocation> verify = Verifier.Times(1).Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "static Target::method(String<>)", +1);
        }

        //[TestMethod]
        public void no_call_and_any_expectation_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<string>())).ThenDefault();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method(Arg.Any<string>())).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }
    }

    //[TestClass]
    public class VerifyTimes
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        public class Target
        {
            public static int method(int i)
            {
                return 0;
            }
        }

        StackFrame stackFrame;

        private void AssertInvocation(Verifier.HandledInvocation handledInvocation, string method, int line)
        {
            Assert.AreEqual(handledInvocation.Dump(0), string.Format(method + " called at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + line));
        }

        private void AssertInvocation(Verifier.HandledInvocation handledInvocation, string hit, int hitLine, string method, int line)
        {
            string calledAt = string.Format(method + " called at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + line);
            string hitFrom = string.Format("{0} from {1}:{2} => ", hit, stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + hitLine);
            Assert.AreEqual(handledInvocation.Dump(0), hitFrom + calledAt);
        }

        [TestMethod]
        public void no_call_and_no_expectation()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Assert.AreEqual(0, Verifier.Times(0).Call(() => Target.method(Arg.Any<int>())).Verify().Count);
        }

        //[TestMethod]
        public void _0_call_and_any_expectation_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method(Arg.Any<int>())).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        //[TestMethod]
        public void _1_call_and_same_times_hit_passed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method(0);

            List<Verifier.HandledInvocation> verify = Verifier.Times(1).Call(() => Target.method(0)).Verify();

            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +3, "static Target::method(Int32<0>)", +1);
        }

        //[TestMethod]
        public void _1_call_and_0_miss_passed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method(0);

            List<Verifier.HandledInvocation> verify = Verifier.Times(0).Call(() => Target.method(1)).Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "static Target::method(Int32<0>)", +1);
        }

        //[TestMethod]
        public void _1_call_and_1_miss_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method(1)).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
    static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        //[TestMethod]
        public void _1_call_and_0_hit_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(0).Call(() => Target.method(0)).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
Expected to call 0 times, but actually call 1 times.", exception.Message);
        }

        //[TestMethod]
        public void _1_call_and_2_hit_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(2).Call(() => Target.method(0)).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
Expected to call 2 times, but actually call 1 times.", exception.Message);
        }

        //[TestMethod]
        public void _2_call_and_1_hit_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);
            Target.method(0);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method(0)).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 5}
    hit(2) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
Expected to call 1 times, but actually call 2 times.", exception.Message);
        }

        //[TestMethod]
        public void _2_call_and_2_hit_passed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);
            Target.method(0);

            Verifier.Times(2).Call(() => Target.method(0)).Verify();
        }

        //[TestMethod]
        public void _2_diff_call_and_2_hit_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);
            Target.method(1);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(2).Call(() => Target.method(0)).Verify();
            });

            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
All invocations:
    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 5}
    {new String(' ', $"hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}".Length)}    static Target::method(Int32<1>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
Expected to call 2 times, but actually call 1 times.", exception.Message);
        }

        //[TestMethod]
        public void hit_rule_2_diff_calls_and_1_times_hit_first_1_times_hit_second_passed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);
            Target.method(1);

            Verifier.Times(1)
                .Call(() => Target.method(0))
                .Call(() => Target.method(1))
                .Verify();
        }

        //[TestMethod]
        public void hit_rule_2_diff_calls_and_1_times_hit_first_1_times_no_hit_failed()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

            Target.method(0);
            Target.method(1);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1)
                    .Call(() => Target.method(0))
                    .Call(() => Target.method(0))
                    .Verify();
            });
            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 3}
All invocations:
    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 2} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 5}
    {new String(' ', $"hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 2}".Length)}    static Target::method(Int32<1>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        //        //[TestMethod]
        //        public void hit_rule_2_diff_calls_and_1_times_hit_second_1_times_no_hit_failed()
        //        {
        //            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

        //            Target.method(0);
        //            Target.method(1);

        //            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
        //            {
        //                stackFrame = new StackTrace(true).GetFrame(0);
        //                Verifier.Times(1)
        //                    .Call(() => Target.method(1))
        //                    .Call(() => Target.method(2))
        //                    .Verify();
        //            });
        //            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
        //All invocations:
        //    {new String(' ', $"hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}".Length)}    static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 5}
        //    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<1>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
        //Expected to call 1 times, but actually call 0 times.", exception.Message);
        //        }

        //        //[TestMethod]
        //        public void hit_rule_2_diff_calls_and_1_times_no_hit_1_times_hit_first_failed()
        //        {
        //            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

        //            Target.method(0);
        //            Target.method(1);

        //            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
        //            {
        //                stackFrame = new StackTrace(true).GetFrame(0);
        //                Verifier.Times(1)
        //                    .Call(() => Target.method(2))
        //                    .Call(() => Target.method(0))
        //                    .Verify();
        //            });
        //            Assert.AreEqual($@"Unsatisfied invocation at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}
        //All invocations:
        //    hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1} => static Target::method(Int32<0>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 5}
        //    {new String(' ', $"hit(1) from {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() + 1}".Length)}    static Target::method(Int32<1>) called at {stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber() - 4}
        //Expected to call 1 times, but actually call 0 times.", exception.Message);
        //        }
    }

}
