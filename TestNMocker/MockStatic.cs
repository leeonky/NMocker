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
        public void should_raise_error_when_unexpected_calls()
        {
            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.NCalls(1, () => Target.method());
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        [TestMethod]
        public void should_record_method_call_in_stub()
        {
            Mocker.When(() => Target.method()).ThenReturn(5);

            Assert.AreEqual(5, Target.method());

            Verifier.NCalls(1, () => Target.method());
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
                Verifier.NCalls(1, () => Target.method1(3));
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
    static Target::method1(Int32<1>)
    static Target::method1(Int32<2>)
Expected to call 1 times, but actually call 0 times.", exception.Message);
        }

        [TestMethod]
        public void verify_method_with_args()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

            Target.method1(1);
            Target.method1(2);

            Verifier.NCalls(1, () => Target.method1(2));
        }

        [TestMethod]
        public void raise_error_when_unsatisfied_verification_of_times()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

            Target.method1(1);
            Target.method1(2);
            Target.method1(2);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.NCalls(1, () => Target.method1(2));
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
    static Target::method1(Int32<1>)
--->static Target::method1(Int32<2>)
--->static Target::method1(Int32<2>)
Expected to call 1 times, but actually call 2 times.", exception.Message);
        }

        [TestMethod]
        public void raise_error_with_no_calls()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

            Target.method1(2);

            UnexpectedCallException exception = Assert.ThrowsException<UnexpectedCallException>(() =>
            {
                stackFrame = new StackTrace(true).GetFrame(0);
                Verifier.NoCalls(() => Target.method1(2));
            });

            Assert.AreEqual(string.Format("Unsatisfied invocation verification at {0}:{1}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber() + 1) + @"
All invocations:
--->static Target::method1(Int32<2>)
Expected no call, but actually call 1 times.", exception.Message);
        }
    }
}
