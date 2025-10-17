using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;

namespace TestNMocker
{
    [TestClass]
    public class StubPublicMemberMethodWithReturnValue
    {
        public class Target
        {
            public bool called;

            public int method()
            {
                called = true;
                return 100;
            }

            public int invoke_method()
            {
                int res = method();
                return res;
            }
        }

        private Target target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
            target.called = false;
        }

        [TestMethod]
        public void stub_member_method_with_returned_value()
        {
            Assert.AreEqual(100, target.method());
            Assert.IsTrue(target.called);
            target.called = false;

            Mocker.When(() => target.method()).ThenReturn(5);

            Assert.AreEqual(5, target.method());
            Assert.IsFalse(target.called);
        }

        [TestMethod]
        public void invoke_stub_member_method()
        {
            Assert.AreEqual(100, target.method());
            Assert.IsTrue(target.called);
            target.called = false;

            Mocker.When(() => target.method()).ThenReturn(5);

            Assert.AreEqual(5, target.invoke_method());
            Assert.IsFalse(target.called);
        }

        [TestMethod]
        public void reset_stub_after_clear()
        {
            Mocker.When(() => target.method()).ThenReturn(5);
            Mocker.Clear();

            Assert.AreEqual(100, target.method());
            Assert.IsTrue(target.called);
        }

        [TestMethod]
        public void support_stub_by_lambda()
        {
            Mocker.When(() => target.method()).Then(args => 999);

            Assert.AreEqual(999, target.method());
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => target.method()).ThenCallActual();

            Assert.AreEqual(100, target.method());
            Assert.IsTrue(target.called);
        }

        [TestMethod]
        public void later_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => target.method()).ThenReturn(5);
            Mocker.When(() => target.method()).ThenReturn(10);

            Assert.AreEqual(10, target.method());
        }

        [TestMethod]
        public void support_call_default()
        {
            Mocker.When(() => target.method()).ThenDefault();

            Assert.AreEqual(0, target.method());
            Assert.IsFalse(target.called);
        }
    }

    [TestClass]
    public class StubPublicMemberMethodByArg
    {
        public class Target
        {
            public bool called;

            public int method(int i)
            {
                called = true;
                return 100;
            }

            public int method(string s)
            {
                return 200;
            }

            public int methodArg(int i)
            {
                return i;
            }
        }

        private Target target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
            target.called = false;
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(() => target.method(1)).ThenReturn(5);
            Mocker.When(() => target.method(2)).ThenReturn(10);
            Mocker.When(() => target.method("hello")).ThenReturn(20);
            Mocker.When(() => target.method("world")).ThenReturn(30);

            Assert.AreEqual(5, target.method(1));
            Assert.AreEqual(10, target.method(2));
            Assert.AreEqual(20, target.method("hello"));
            Assert.AreEqual(30, target.method("world"));
            Assert.AreEqual(100, target.method(100));
            Assert.AreEqual(200, target.method("xxx"));
        }

        [TestMethod]
        public void use_var_in_method_arg_match()
        {
            int i = 1;
            Mocker.When(() => target.method(i)).ThenReturn(5);
            Assert.AreEqual(5, target.method(1));
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(() => target.method(Arg.Is(1))).ThenReturn(5);
            Assert.AreEqual(5, target.method(1));
            Assert.AreEqual(100, target.method(2));
        }

        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(() => target.method(Arg.Any<int>())).ThenReturn(5);
            Assert.AreEqual(5, target.method(1));
            Assert.AreEqual(5, target.method(2));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(() => target.method(Arg.That<int>(i => i > 5))).ThenReturn(5);
            Assert.AreEqual(5, target.method(6));
            Assert.AreEqual(100, target.method(2));
        }

        [TestMethod]
        public void support_call_actual_when_arg_match()
        {
            Mocker.When(() => target.methodArg(Arg.Any<int>())).ThenCallActual();
            Assert.AreEqual(1, target.methodArg(1));
            Assert.AreEqual(2, target.methodArg(2));
            Assert.AreEqual(10, target.methodArg(10));
        }

        [TestMethod]
        public void later_matched_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => target.method(Arg.Any<int>())).ThenReturn(5);
            Mocker.When(() => target.method(1)).ThenReturn(10);

            Assert.AreEqual(5, target.method(2));
            Assert.AreEqual(10, target.method(1));
        }

        [TestMethod]
        public void use_passed_args_in_lambda()
        {
            Mocker.When(() => target.method(Arg.Any<int>())).Then(args => ((int)args[0]) + 1);

            Assert.AreEqual(2, target.method(1));
            Assert.AreEqual(3, target.method(2));
        }

        [TestMethod]
        public void args_can_passed_to_original_method_when_call_actual()
        {
            Mocker.When(() => target.methodArg(Arg.Any<int>())).ThenCallActual();

            Assert.AreEqual(1, target.methodArg(1));
            Assert.AreEqual(2, target.methodArg(2));
            Assert.AreEqual(10, target.methodArg(10));
        }
    }

    [TestClass]
    public class StubPublicMemberMethodByRefArg
    {
        public class Target
        {
            public bool called;
            public int method(ref int i)
            {
                i += 100;
                called = true;
                return 100;
            }
        }

        private Target target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
            target.called = false;
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.method(ref i));
            Assert.IsFalse(target.called);
        }

        [TestMethod]
        public void support_arg_any_match()
        {
            Mocker.When(typeof(Target), "method", Arg.Any<int>().Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.method(ref i));
        }

        [TestMethod]
        public void support_customer_arg_match()
        {
            Mocker.When(typeof(Target), "method", Arg.That<int>(i => i > 5).Ref()).ThenReturn(5);

            int v = 6;
            Assert.AreEqual(5, target.method(ref v));
        }

        [TestMethod]
        public void can_assign_ref_value_in_lambda()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(1).Ref()).Then(args => args[0] = 999);

            int i = 1;
            target.method(ref i);
            Assert.AreEqual(999, i);
        }

        [TestMethod]
        public void do_not_assign_ref_value_in_lambda_when_arg_not_matched()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(1).Ref()).Then(args => args[0] = 999);

            int i = 100;
            target.method(ref i);
            Assert.AreEqual(200, i);
        }

        [TestMethod]
        public void can_assign_ref_value_in_actual_call()
        {
            Mocker.When(typeof(Target), "method", Arg.Any<int>().Ref()).ThenCallActual();

            int v = 6;
            Assert.AreEqual(100, target.method(ref v));
            Assert.AreEqual(106, v);
        }

        [TestMethod]
        public void can_assign_ref_value_directly()
        {
            Mocker.When(typeof(Target), "method", Arg.Is(10).Ref(1000)).ThenDefault();

            int v = 10;
            target.method(ref v);
            Assert.AreEqual(1000, v);
        }
    }

    [TestClass]
    public class StubPublicMemberMethodByOutArg
    {
        public class Target
        {
            public int method(out int i)
            {
                i = 1000;
                return 100;
            }
        }

        private Target target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
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
            Assert.AreEqual(999, target.method(out i));
            Assert.AreEqual(1000, i);
        }

        [TestMethod]
        public void support_out_arg_directly()
        {
            Mocker.When(typeof(Target), "method", Arg.Out(1000)).ThenDefault();

            int i;
            target.method(out i);

            Assert.AreEqual(1000, i);
        }
    }

    [TestClass]
    public class StubMemberWithTypeAndMethodName
    {
        public class Target
        {
            public int method(string msg)
            {
                return 100;
            }

            public int method2(string msg)
            {
                return 100;
            }

            public int method2(object obj)
            {
                return 100;
            }

            public int method3(int i)
            {
                return 100;
            }

            public int method3(object obj)
            {
                return 100;
            }

            public int method4(int? i)
            {
                return 100;
            }

        }

        private Target target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
        }

        [TestMethod]
        public void can_use_const_null_arg_value_in_non_primitive_parameter()
        {
            Mocker.When(typeof(Target), "method", null).ThenReturn(1);

            Assert.AreEqual(1, target.method(null));
            Assert.AreEqual(100, target.method("xxx"));
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

            Assert.AreEqual(1, target.method3(null));
            Assert.AreEqual(100, target.method3(0));
        }

        [TestMethod]
        public void null_can_distinguish_nullable()
        {
            Mocker.When(typeof(Target), "method4", null).ThenReturn(1);

            Assert.AreEqual(1, target.method4(null));
            Assert.AreEqual(100, target.method4(0));
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
    public class StubVoidMemberMethod
    {
        public class Target
        {
            public int value;

            public void method(int i)
            {
                value = i;
            }
        }

        private Target target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
            target.value = 0;
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => target.method(10)).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void support_call_default()
        {
            Mocker.When(() => target.method(10)).ThenDefault();
            target.method(10);
            Assert.AreEqual(0, target.value);
        }

        [TestMethod]
        public void support_lambda()
        {
            Mocker.When(() => target.method(10)).Then(objs => target.value = 99);
            target.method(10);
            Assert.AreEqual(99, target.value);
        }

        [TestMethod]
        public void support_call_actual_with_typeof_and_method_name()
        {
            Mocker.WhenVoid(typeof(Target), "method", Arg.Any<int>()).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void support_call_default_with_typeof_and_method_name()
        {
            Mocker.WhenVoid(typeof(Target), "method", Arg.Any<int>()).ThenDefault();
            target.method(10);
            Assert.AreEqual(0, target.value);
        }

        [TestMethod]
        public void support_reference_method_by_string()
        {
            Mocker.WhenVoid(typeof(Target), "method", 10).Then(objs => target.value = 99);

            target.method(10);

            Assert.AreEqual(99, target.value);
        }
    }

}