using Microsoft.VisualStudio.TestTools.UnitTesting;
using nmocker;

namespace TestNMocker
{
    [TestClass]
    public class StubPublicMethodWithReturnValue
    {
        public class Target
        {
            public static bool called;
            public static int method()
            {
                called = true;
                return 100;
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
            Target.called = false;
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
        public void invoke_stub_method()
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
        public void support_stub_by_lambda()
        {
            Mocker.When(() => Target.method()).Then(args => 999);

            Assert.AreEqual(999, Target.method());
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => Target.method()).ThenCallActual();

            Assert.AreEqual(100, Target.method());
            Assert.IsTrue(Target.called);
        }

        [TestMethod]
        public void later_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => Target.method()).ThenReturn(5);
            Mocker.When(() => Target.method()).ThenReturn(10);

            Assert.AreEqual(10, Target.method());
        }
    }

    [TestClass]
    public class StubPublicMethodByArg
    {
        public class Target
        {
            public static bool called;

            public static int method(int i)
            {
                called = true;
                return 100;
            }
            public static int method(string s)
            {
                return 200;
            }

            public static int methodArg(int i)
            {
                return i;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Target.called = false;
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(() => Target.method(1)).ThenReturn(5);
            Mocker.When(() => Target.method(2)).ThenReturn(10);
            Mocker.When(() => Target.method("hello")).ThenReturn(20);
            Mocker.When(() => Target.method("world")).ThenReturn(30);

            Assert.AreEqual(5, Target.method(1));
            Assert.AreEqual(10, Target.method(2));
            Assert.AreEqual(20, Target.method("hello"));
            Assert.AreEqual(30, Target.method("world"));

            Assert.AreEqual(0, Target.method(100));
            Assert.AreEqual(0, Target.method("xxx"));
        }

        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(() => Target.method(Arg<int>.Any())).ThenReturn(5);

            Assert.AreEqual(5, Target.method(1));
            Assert.AreEqual(5, Target.method(2));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(() => Target.method(Arg<int>.That(i => i > 5))).ThenReturn(5);

            Assert.AreEqual(0, Target.method(4));
            Assert.AreEqual(0, Target.method(5));
            Assert.AreEqual(5, Target.method(6));
        }

        [TestMethod]
        public void later_matched_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => Target.method(Arg<int>.Any())).ThenReturn(5);
            Mocker.When(() => Target.method(1)).ThenReturn(10);

            Assert.AreEqual(5, Target.method(2));
            Assert.AreEqual(10, Target.method(1));
        }

        [TestMethod]
        public void use_passed_args_in_lambda()
        {
            Mocker.When(() => Target.method(Arg<int>.Any())).Then(args => ((int)args[0]) + 1);

            Assert.AreEqual(2, Target.method(1));
            Assert.AreEqual(3, Target.method(2));
        }

        [TestMethod]
        public void args_can_passed_to_original_method_when_call_actual()
        {
            Mocker.When(() => Target.methodArg(Arg<int>.Any())).ThenCallActual();

            Assert.AreEqual(1, Target.methodArg(1));
            Assert.AreEqual(2, Target.methodArg(2));
            Assert.AreEqual(10, Target.methodArg(10));
        }
    }

    [TestClass]
    public class StubPublicMethodByRefArg
    {
        public class Target
        {
            public static int method(ref int i)
            {
                i = 1000;
                return 100;
            }
            public static int method(ref string s)
            {
                s = "ref";
                return 200;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        /*
        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(() => Target.method(Arg<int>.Any())).ThenReturn(5);

            Assert.AreEqual(5, Target.method(1));
            Assert.AreEqual(5, Target.method(2));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(() => Target.method(Arg<int>.That(i => i > 5))).ThenReturn(5);

            Assert.AreEqual(0, Target.method(4));
            Assert.AreEqual(0, Target.method(5));
            Assert.AreEqual(5, Target.method(6));
        }

        [TestMethod]
        public void later_matched_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => Target.method(Arg<int>.Any())).ThenReturn(5);
            Mocker.When(() => Target.method(1)).ThenReturn(10);

            Assert.AreEqual(5, Target.method(2));
            Assert.AreEqual(10, Target.method(1));
        }

        [TestMethod]
        public void use_passed_args_in_lambda()
        {
            Mocker.When(() => Target.method(Arg<int>.Any())).Then(args => ((int)args[0]) + 1);

            Assert.AreEqual(2, Target.method(1));
            Assert.AreEqual(3, Target.method(2));
        }

        [TestMethod]
        public void args_can_passed_to_original_method_when_call_actual()
        {
            Mocker.When(() => Target.methodArg(Arg<int>.Any())).ThenCallActual();

            Assert.AreEqual(1, Target.methodArg(1));
            Assert.AreEqual(2, Target.methodArg(2));
            Assert.AreEqual(10, Target.methodArg(10));
        }
        */
    }
}
