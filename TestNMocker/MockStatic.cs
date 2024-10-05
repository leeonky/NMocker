using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestNMocker
{
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
            Assert.AreEqual(string.Format(method + " called at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + line), handledInvocation.Dump(0) );
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
            Target.method("");

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

        [TestMethod]
        public void a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1).Call(() => Target.method("a")).Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +3, "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_hit_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier.Times(0).Call(() => Target.method("b")).Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(0).Call(() => Target.method("a"))
                .Times(1).Call(() => Target.method("b"))
                .Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +4, "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_hit_b_and_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(0).Call(() => Target.method("b"))
                .Times(1).Call(() => Target.method("a"))
                .Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +5, "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_hit_a_any()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method(Arg.Any<string>()))
                .Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +4, "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_hit_b_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(0).Call(() => Target.method("b"))
                .Times(0).Call(() => Target.method("b"))
                .Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +4, "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
            Assert.AreEqual(2, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +5, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "hit(1)", +6, "static Target::method(String<b>)", +2);
        }

        [TestMethod]
        public void a_x_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("x");
            Target.method("b");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Verify();
            Assert.AreEqual(3, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +6, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "static Target::method(String<x>)", +2);
            AssertInvocation(verify[2], "hit(1)", +7, "static Target::method(String<b>)", +3);
        }

        [TestMethod]
        public void a_b_hit_a_x()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("x"))
                .Verify();
            Assert.AreEqual(2, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +5, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "static Target::method(String<b>)", +2);
        }

        [TestMethod]
        public void a_b_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(1).Call(() => Target.method("a"))
                .Times(1).Call(() => Target.method("b"))
                .Verify();
            Assert.AreEqual(2, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +5, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "hit(1)", +6, "static Target::method(String<b>)", +2);
        }

        [TestMethod]
        public void a_b_c_d_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");
            Target.method("c");
            Target.method("d");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Times(1)
                .Call(() => Target.method("c"))
                .Call(() => Target.method("d"))
                .Verify();
            Assert.AreEqual(4, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +8, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "hit(1)", +9, "static Target::method(String<b>)", +2);
            AssertInvocation(verify[2], "hit(1)", +11, "static Target::method(String<c>)", +3);
            AssertInvocation(verify[3], "hit(1)", +12, "static Target::method(String<d>)", +4);
        }

        [TestMethod]
        public void a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("c");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(1)
                .Call(() => Target.method("a"))
                .Call(() => Target.method("b"))
                .Times(1)
                .Call(() => Target.method("c"))
                .Call(() => Target.method("d"))
                .Verify();
            Assert.AreEqual(2, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +6, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "hit(1)", +9, "static Target::method(String<c>)", +2);
        }

        [TestMethod]
        public void a_b_c_d_hit_a_and_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("b");
            Target.method("c");
            Target.method("d");

            List<Verifier.HandledInvocation> verify = Verifier
                .Times(1)
                .Call(() => Target.method("a"))
                .Times(1)
                .Call(() => Target.method("c"))
                .Verify();
            Assert.AreEqual(4, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +8, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "static Target::method(String<b>)", +2);
            AssertInvocation(verify[2], "hit(1)", +10, "static Target::method(String<c>)", +3);
            AssertInvocation(verify[3], "static Target::method(String<d>)", +4);
        }

        [TestMethod]
        public void a_hit_b_x_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier.Times(1)
                .Call(() => Target.method("b"))
                .Call(() => Target.method("x"))
                .Call(() => Target.method("a"))
                .Verify();
            Assert.AreEqual(1, verify.Count);
            AssertInvocation(verify[0], "static Target::method(String<a>)", +1);
        }

        [TestMethod]
        public void a_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            Target.method("a");
            Target.method("a");

            List<Verifier.HandledInvocation> verify = Verifier.Times(2).Call(() => Target.method("a")).Verify();
            Assert.AreEqual(2, verify.Count);
            AssertInvocation(verify[0], "hit(1)", +4, "static Target::method(String<a>)", +1);
            AssertInvocation(verify[1], "hit(2)", +4, "static Target::method(String<a>)", +2);
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
            Assert.AreEqual(handledInvocation.Dump(0), hitFrom+calledAt);
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
