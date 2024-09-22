using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System.Diagnostics;

namespace TestNMocker
{
    [TestClass]
    public class MockPublicStaticMethod
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        public class Target
        {
            public static int method()
            {
                return 100;
            }

            public static int method1(int i)
            {
                return 100;
            }
        }

        StackFrame stackFrame;
        [TestMethod]
        public void should_record_method_call_in_stub_and_should_raise_error_when_unexpected_calls()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method()).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        [TestMethod]
        public void should_pass_when_call_matched()
        {
            Mocker.When(() => Target.method()).ThenReturn(5);

            Assert.AreEqual(5, Target.method());

            Verifier.Times(1).Call(() => Target.method()).Verify();
        }

        [TestMethod]
        public void raise_error_when_unsatisfied_verification_with_args()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

            Target.method1(1);
            Target.method1(2);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method1(3)).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
    static Target::method1(Int32<1>)
    static Target::method1(Int32<2>)
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        [TestMethod]
        public void should_passed_when_method_matched_with_args()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

            Target.method1(1);
            Target.method1(2);

            Verifier.Times(1).Call(() => Target.method1(2)).Verify();
        }
    }

    [TestClass]
    public class VerifyInSequence
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

        [TestMethod]
        public void raise_error_when_unsatisfied_verification_of_times()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenReturn(5);

            Target.method(1);
            Target.method(2);
            Target.method(2);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Times(1).Call(() => Target.method(2)).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
    static Target::method(Int32<1>)
--->static Target::method(Int32<2>)
--->static Target::method(Int32<2>)
Expected to call 1 times, but actually call 2 times.", exception.Message);
        }

//        [TestMethod]
//        public void verify_one_section_in_order_passed()
//        {
//            Mocker.When(() => Target.method(Arg.Any<int>())).ThenDefault();

//            Target.method(1);
//            Target.method(2);

//            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
//            {
//                stackFrame = new StackTrace(true).GetFrame(0);
//                Verifier.Once
//                    .Call(() => Target.method(2))
//                    .Call(() => Target.method(1))
//                    .Verify();
//            });

//            Assert.AreEqual(string.Format("Incorrect order of invocation at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 3) + @"
//All invocations:
//??->static Target::method(Int32<1>)
//--->static Target::method(Int32<2>)
//Expected to call at least 1 times, but actually call 0 times.", exception.Message);
//        }
    }

    [TestClass]
    public class MoreConvenientApi
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        public class Target
        {
            public static int method()
            {
                return 100;
            }
        }

        StackFrame stackFrame;

        [TestMethod]
        public void verify_once()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Once.Call(() => Target.method()).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        [TestMethod]
        public void verify_never()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            Target.method();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Never.Call(() => Target.method()).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
--->static Target::method()
Expected no call, but actually call 1 times.", exception.Message);
        }

        [TestMethod]
        public void verify_at_least_failed()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.AtLeast(1).Call(() => Target.method()).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
Expected to call at least 1 times, but actually call 0 times.", exception.Message);
        }

        [TestMethod]
        public void verify_at_least_passed()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            Target.method();

            Verifier.AtLeast(1).Call(() => Target.method()).Verify();

            Target.method();

            Verifier.AtLeast(1).Call(() => Target.method()).Verify();
        }

        [TestMethod]
        public void verify_at_most_failed()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            Target.method();
            Target.method();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.AtMost(1).Call(() => Target.method()).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
--->static Target::method()
--->static Target::method()
Expected to call at most 1 times, but actually call 2 times.", exception.Message);
        }

        [TestMethod]
        public void verify_at_most_passed()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            Verifier.AtMost(1).Call(() => Target.method()).Verify();

            Target.method();

            Verifier.AtMost(1).Call(() => Target.method()).Verify();
        }

        [TestMethod]
        public void default_verify_at_least_once()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.Call(() => Target.method()).Verify();
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
Expected to call at least 1 times, but actually call 0 times.", exception.Message);
        }
    }
}
