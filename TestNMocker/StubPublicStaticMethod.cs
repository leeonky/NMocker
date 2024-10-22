using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;

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

        [TestMethod]
        public void support_call_default()
        {
            Mocker.When(() => Target.method()).ThenDefault();

            Assert.AreEqual(0, Target.method());
            Assert.IsFalse(Target.called);
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

            Assert.AreEqual(100, Target.method(100));
            Assert.AreEqual(200, Target.method("xxx"));
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(() => Target.method(Arg.Is(1))).ThenReturn(5);

            Assert.AreEqual(5, Target.method(1));
            Assert.AreEqual(100, Target.method(2));
        }

        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenReturn(5);

            Assert.AreEqual(5, Target.method(1));
            Assert.AreEqual(5, Target.method(2));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(() => Target.method(Arg.That<int>(i => i > 5))).ThenReturn(5);

            Assert.AreEqual(100, Target.method(4));
            Assert.AreEqual(100, Target.method(5));
            Assert.AreEqual(5, Target.method(6));
        }

        [TestMethod]
        public void later_matched_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).ThenReturn(5);
            Mocker.When(() => Target.method(1)).ThenReturn(10);

            Assert.AreEqual(5, Target.method(2));
            Assert.AreEqual(10, Target.method(1));
        }

        [TestMethod]
        public void use_passed_args_in_lambda()
        {
            Mocker.When(() => Target.method(Arg.Any<int>())).Then(args => ((int)args[0]) + 1);

            Assert.AreEqual(2, Target.method(1));
            Assert.AreEqual(3, Target.method(2));
        }

        [TestMethod]
        public void args_can_passed_to_original_method_when_call_actual()
        {
            Mocker.When(() => Target.methodArg(Arg.Any<int>())).ThenCallActual();

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
                i += 100;
                return 100;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, Target.method(ref i));
        }

        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(typeof(Target), "method", Arg.Any<int>().Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, Target.method(ref i));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(typeof(Target), "method", Arg.That<int>(i => i > 5).Ref()).ThenReturn(5);

            int v = 6;
            Assert.AreEqual(5, Target.method(ref v));
        }

        [TestMethod]
        public void can_assign_ref_value_in_lambda()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(1).Ref()).Then(args => args[0] = 999);

            int i = 1;
            Target.method(ref i);
            Assert.AreEqual(999, i);
        }

        [TestMethod]
        public void do_not_assign_ref_value_in_lambda_when_arg_not_matched()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(1).Ref()).Then(args => args[0] = 999);

            int i = 100;
            Target.method(ref i);
            Assert.AreEqual(200, i);
        }

        [TestMethod]
        public void can_assign_ref_value_in_actual_call()
        {
            Mocker.When(typeof(Target), "method", Arg.Any<int>().Ref()).ThenCallActual();

            int v = 6;
            Assert.AreEqual(100, Target.method(ref v));
            Assert.AreEqual(106, v);
        }

        [TestMethod]
        public void can_assign_ref_value_directly()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(10).Ref(1000)).ThenDefault();

            int v = 10;
            Target.method(ref v);
            Assert.AreEqual(1000, v);
        }
    }

    [TestClass]
    public class StubPublicMethodByOutArg
    {
        public class Target
        {
            public static int method(out int i)
            {
                i = 1000;
                return 100;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        [TestMethod]
        public void support_out_arg_by_lambda()
        {
            Mocker.When(typeof(Target), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, Target.method(out i));
            Assert.AreEqual(1000, i);
        }

        [TestMethod]
        public void support_out_arg_directly()
        {
            Mocker.When(typeof(Target), "method", Arg.Out(1000)).ThenDefault();

            int i;
            Target.method(out i);

            Assert.AreEqual(1000, i);
        }
    }

    [TestClass]
    public class StubWithTypeAndMethodName
    {
        public class Target
        {
            public static int method(string msg)
            {
                return 100;
            }

            public static int method2(string msg)
            {
                return 100;
            }

            public static int method2(object obj)
            {
                return 100;
            }

            public static int method3(int i)
            {
                return 100;
            }

            public static int method3(object obj)
            {
                return 100;
            }

            public static int method4(int? i)
            {
                return 100;
            }
        }

        [TestMethod]
        public void can_use_const_null_arg_value_in_non_primitive_parameter()
        {
            Mocker.When(typeof(Target), "method", null).ThenReturn(1);

            Assert.AreEqual(1, Target.method(null));
            Assert.AreEqual(100, Target.method("xxx"));
        }

        [TestMethod]
        public void raise_error_when_more_than_one_matched_methods()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
            {
                Mocker.When(typeof(Target), "method2", null).ThenReturn(1);
            });

            Assert.AreEqual(@"Ambiguous method between the following:
    Target::method2(String)
    Target::method2(Object)", exception.Message);
        }

        [TestMethod]
        public void null_can_distinguish_between_primitive_object_types()
        {
            Mocker.When(typeof(Target), "method3", null).ThenReturn(1);

            Assert.AreEqual(1, Target.method3(null));
            Assert.AreEqual(100, Target.method3(0));
        }

        [TestMethod]
        public void null_can_distinguish_nullable()
        {
            Mocker.When(typeof(Target), "method4", null).ThenReturn(1);

            Assert.AreEqual(1, Target.method4(null));
            Assert.AreEqual(100, Target.method4(0));
        }

        [TestMethod]
        public void raise_error_when_no_matching_method()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
            {
                Mocker.When(typeof(Target), "method4", string.Empty).ThenReturn(1);
            });

            Assert.AreEqual("No matching method found", exception.Message);
        }
    }

    [TestClass]
    public class StubVoidMethod
    {
        public class Target
        {
            public static int value;
            public static void method(int i)
            {
                value = i;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Target.value = 0;
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => Target.method(10)).ThenCallActual();

            Target.method(10);

            Assert.AreEqual(10, Target.value);
        }

        [TestMethod]
        public void support_call_default()
        {
            Mocker.When(() => Target.method(10)).ThenDefault();

            Target.method(10);

            Assert.AreEqual(0, Target.value);
        }

        [TestMethod]
        public void support_lambda()
        {
            Mocker.When(() => Target.method(10)).Then(objs => Target.value = 99);

            Target.method(10);

            Assert.AreEqual(99, Target.value);
        }

        [TestMethod]
        public void support_reference_method_by_string()
        {
            Mocker.WhenVoid(typeof(Target), "method", 10).Then(objs => Target.value = 99);

            Target.method(10);

            Assert.AreEqual(99, Target.value);
        }
    }
}
