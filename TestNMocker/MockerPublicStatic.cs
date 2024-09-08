using Microsoft.VisualStudio.TestTools.UnitTesting;
using nmocker;

namespace mockerTest
{
    [TestClass]
    public class MockerPublicStaticMethod
    {
        public class Target
        {
            public static bool called;
            public static int method()
            {
                called = true;
                return 100;
            }

            public static int method1(int i)
            {
                return 100;
            }
            public static int method1(string s)
            {
                return 200;
            }

            public static int invoke_method()
            {
                int res = method();
                return res;
            }
        }


        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        [TestMethod]
        public void stub_static_method_with_returned_value()
        {
            Assert.AreEqual(100, Target.method());
            Assert.IsTrue(Target.called);

            Target.called = false;

            Mocker.When(() => Target.method()).ThenReturn(5);

            Assert.AreEqual(5, Target.method());
            Assert.IsFalse(Target.called);
        }

        [TestMethod]
        public void invoke_stub_static_method_with_returned_value()
        {
            Assert.AreEqual(100, Target.method());
            Assert.IsTrue(Target.called);

            Target.called = false;

            Mocker.When(() => Target.method()).ThenReturn(5);

            Assert.AreEqual(5, Target.invoke_method());
            Assert.IsFalse(Target.called);
        }

        [TestMethod]
        public void reset_stub_after_clear()
        {
            Mocker.When(() => Target.method()).ThenReturn(5);
            Mocker.Clear();

            Assert.AreEqual(100, Target.method());
            Assert.IsTrue(Target.called);
        }

        [TestMethod]
        public void stub_static_method_with_returned_value_and_args()
        {
            Mocker.When(() => Target.method1(1)).ThenReturn(5);
            Mocker.When(() => Target.method1(2)).ThenReturn(10);

            Assert.AreEqual(5, Target.method1(1));
            Assert.AreEqual(10, Target.method1(2));
        }

        [TestMethod]
        public void override_privours_stub()
        {
            Mocker.When(() => Target.method1(1)).ThenReturn(5);
            Mocker.When(() => Target.method1(1)).ThenReturn(10);

            Assert.AreEqual(10, Target.method1(1));
        }

        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

            Assert.AreEqual(5, Target.method1(1));
            Assert.AreEqual(5, Target.method1(2));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(() => Target.method1(Arg.That<int>(i => i > 5))).ThenReturn(5);

            Assert.AreEqual(0, Target.method1(4));
            Assert.AreEqual(0, Target.method1(5));
            Assert.AreEqual(5, Target.method1(6));
        }

        [TestMethod]
        public void support_stub_by_lambda()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).Then(args => ((int)args[0]) + 1);

            Assert.AreEqual(2, Target.method1(1));
            Assert.AreEqual(3, Target.method1(2));
        }


        [TestMethod]
        public void support_call_real()
        {
            Mocker.When(() => Target.method1(Arg.Any<int>())).Then(args => ((int)args[0]) + 1);
            Mocker.When(() => Target.method1(10)).ThenCallActual();

            Assert.AreEqual(2, Target.method1(1));
            Assert.AreEqual(3, Target.method1(2));
            Assert.AreEqual(100, Target.method1(10));
        }
    }
}
