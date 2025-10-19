using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestNMocker
{
    [TestClass]
    public class TestVerifyTimesAndHitMember : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.Mock(() => new Target().method(Arg.Any<string>()));
        }

        public class Target
        {
            public int method(string s)
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
                Verifier.Times(1).Called(() => new Target().method("a")).Verify();
            });

            VerifyMessage(4,
                Expected("Expected to call 1 times, but actually call 0 times", 1));

            Verifier.Times(0).Called(() => new Target().method("a")).Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(0).Called(() => new Target().method("a")).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1));

            Verifier.Times(1).Called(() => new Target().method("a")).Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_hit_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Called(() => new Target().method("b")).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Invocation("Target::method(String<a>)", 1));

            Verifier.Times(0).Called(() => new Target().method("b")).Verify();
        }

        [TestMethod]
        public void excess_group_and_hit_group_a_hit_b_and_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(1).Called(() => new Target().method("b"))
                .Times(0).Called(() => new Target().method("a"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 2, "Target::method(String<a>)", 1));

            Verifier
                .Times(0).Called(() => new Target().method("b"))
                .Times(1).Called(() => new Target().method("a"))
                .Verify();
        }

        [TestMethod]
        public void hit_group_and_excess_group_a_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(0).Called(() => new Target().method("a"))
                .Times(1).Called(() => new Target().method("b"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1));

            Verifier
                .Times(1).Called(() => new Target().method("a"))
                .Times(0).Called(() => new Target().method("b"))
                .Verify();
        }

        [TestMethod]
        public void miss_group_and_excess_group_a_hit_b_and_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier
                .Times(1).Called(() => new Target().method("b"))
                .Times(1).Called(() => new Target().method("c"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("Target::method(String<a>)", 1));

            Verifier
                .Times(0).Called(() => new Target().method("b"))
                .Times(0).Called(() => new Target().method("c"))
                .Verify();
        }

        [TestMethod]
        public void single_group_excess_call_a_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                .Called(() => new Target().method("a"))
                .Called(() => new Target().method("b"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1));

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                .Called(() => new Target().method("a"))
                .Called(() => new Target().method("b"))
                .Verify();
            });

            VerifyMessage(18,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1));
        }

        [TestMethod]
        public void single_group_excess_call_a_hit_x_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                .Called(() => new Target().method("x"))
                .Called(() => new Target().method("a"))
                .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 0 times", 1),
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("Target::method(String<a>)", 1));

            Verifier.Times(0)
                .Called(() => new Target().method("x"))
                .Called(() => new Target().method("a"))
                .Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 2));

            Verifier.Times(1)
                .Called(() => new Target().method("a"))
                .Called(() => new Target().method("b"))
                .Verify();
        }

        [TestMethod]
        public void single_group_balanced_a_b_hit_a_c()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(1)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("c"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("Target::method(String<b>)", 2));

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("c"))
                    .Verify();
            });

            VerifyMessage(20,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("Target::method(String<b>)", 2));
        }

        [TestMethod]
        public void single_group_balanced_a_c_b_hit_a_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("c");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier.Times(0)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(8,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("Target::method(String<c>)", 2),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 3));

            Verifier.Times(1)
                .Called(() => new Target().method("a"))
                .Called(() => new Target().method("b"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_a_b_hit_a_and_b()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0).Called(() => new Target().method("a"))
                    .Times(0).Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 2));

            Verifier
                .Times(1).Called(() => new Target().method("a"))
                .Times(1).Called(() => new Target().method("b"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_a_b_c_d_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("b");
            new Target().method("c");
            new Target().method("d");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("b"))
                    .Times(0)
                    .Called(() => new Target().method("c"))
                    .Called(() => new Target().method("d"))
                    .Verify();
            });

            VerifyMessage(10,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Expected("Expected to call 0 times, but actually call 1 times", 4),
                Expected("Expected to call 0 times, but actually call 1 times", 5),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 2),
                Invocation("hit(1)", 4, "Target::method(String<c>)", 3),
                Invocation("hit(1)", 5, "Target::method(String<d>)", 4));

            Verifier
                .Times(1)
                .Called(() => new Target().method("a"))
                .Called(() => new Target().method("b"))
                .Times(1)
                .Called(() => new Target().method("c"))
                .Called(() => new Target().method("d"))
                .Verify();
        }

        [TestMethod]
        public void two_group_balanced_each_group_excess_verification_a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("c");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("b"))
                    .Times(0)
                    .Called(() => new Target().method("c"))
                    .Called(() => new Target().method("d"))
                    .Verify();
            });

            VerifyMessage(8,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 4),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(1)", 4, "Target::method(String<c>)", 2));

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(1)
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("b"))
                    .Times(1)
                    .Called(() => new Target().method("c"))
                    .Called(() => new Target().method("d"))
                    .Verify();
            });

            VerifyMessage(26,
                Expected("Expected to call 1 times, but actually call 0 times", 2),
                Expected("Expected to call 1 times, but actually call 0 times", 5),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(1)", 4, "Target::method(String<c>)", 2));
        }

        [TestMethod]
        public void two_group_balanced_each_group_excess_call_a_c_hit_a_b_and_c_d()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("b");
            new Target().method("c");
            new Target().method("d");

            ExecuteFailed(() =>
            {
                Verifier
                    .Times(0).Called(() => new Target().method("a"))
                    .Times(0).Called(() => new Target().method("c"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 0 times, but actually call 1 times", 1),
                Expected("Expected to call 0 times, but actually call 1 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("Target::method(String<b>)", 2),
                Invocation("hit(1)", 2, "Target::method(String<c>)", 3),
                Invocation("Target::method(String<d>)", 4));

            Verifier
                .Times(1).Called(() => new Target().method("a"))
                .Times(1).Called(() => new Target().method("c"))
                .Verify();
        }

        [TestMethod]
        public void hit_more_than_once_a_a_hit_a()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier.Times(1).Called(() => new Target().method("a")).Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "Target::method(String<a>)", 2));

            Verifier.Times(2).Called(() => new Target().method("a")).Verify();
        }
    }

    [TestClass]
    public class MoreVerificationApiMember : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Mocker.Mock(() => new Target().method(Arg.Any<string>()));
        }

        public class Target
        {
            public int method(string s)
            {
                return 0;
            }

            public void methodVoid(string s)
            {
            }
        }

        [TestMethod]
        public void at_least()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("a");
            new Target().method("b");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .AtLeast(3).Called(() => new Target().method("a"))
                    .AtLeast(3).Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call at least 3 times, but actually call 2 times", 1),
                Expected("Expected to call at least 3 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "Target::method(String<a>)", 2),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 3),
                Invocation("hit(2)", 2, "Target::method(String<b>)", 4));

            Verifier
                .AtLeast(2).Called(() => new Target().method("a"))
                .AtLeast(2).Called(() => new Target().method("b"))
                .Verify();

            Verifier
                .AtLeast(1).Called(() => new Target().method("a"))
                .AtLeast(1).Called(() => new Target().method("b"))
                .Verify();

        }

        [TestMethod]
        public void once()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("a");
            new Target().method("b");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(() => new Target().method("a"))
                    .Once().Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "Target::method(String<a>)", 2),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 3),
                Invocation("hit(2)", 2, "Target::method(String<b>)", 4)
                );
        }

        [TestMethod]
        public void at_most()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");
            new Target().method("a");
            new Target().method("b");
            new Target().method("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .AtMost(1).Called(() => new Target().method("a"))
                    .AtMost(1).Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call at most 1 times, but actually call 2 times", 1),
                Expected("Expected to call at most 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1),
                Invocation("hit(2)", 1, "Target::method(String<a>)", 2),
                Invocation("hit(1)", 2, "Target::method(String<b>)", 3),
                Invocation("hit(2)", 2, "Target::method(String<b>)", 4)
                );

            Verifier
                .AtMost(2).Called(() => new Target().method("a"))
                .AtMost(2).Called(() => new Target().method("b"))
                .Verify();

            Verifier
                .AtMost(3).Called(() => new Target().method("a"))
                .AtMost(3).Called(() => new Target().method("b"))
                .Verify();
        }

        [TestMethod]
        public void default_is_at_least_call_in_single_group()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().method("a");

            ExecuteFailed(() =>
            {
                Verifier
                    .Called(() => new Target().method("a"))
                    .Called(() => new Target().method("b"))
                    .Verify();
            });

            VerifyMessage(6,
                Expected("Expected to call at least 1 times, but actually call 0 times", 2),
                Invocation("hit(1)", 1, "Target::method(String<a>)", 1));

            Verifier
               .Called(() => new Target().method("a"))
                .Verify();

            Verifier
                .AtLeast(1).Called(() => new Target().method("a"))
                .Verify();

        }

        [TestMethod]
        public void void_method()
        {
            Mocker.Mock(() => new Target().methodVoid(Arg.Any<string>()));
            stackFrame = new StackTrace(true).GetFrame(0);
            new Target().methodVoid("a");
            new Target().methodVoid("a");
            new Target().methodVoid("b");
            new Target().methodVoid("b");

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(() => new Target().methodVoid("a"))
                    .Once().Called(() => new Target().methodVoid("b"))
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 1 times, but actually call 2 times", 1),
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 1, "Target::methodVoid(String<a>)", 1),
                Invocation("hit(2)", 1, "Target::methodVoid(String<a>)", 2),
                Invocation("hit(1)", 2, "Target::methodVoid(String<b>)", 3),
                Invocation("hit(2)", 2, "Target::methodVoid(String<b>)", 4)
                );
        }
    }

    [TestClass]
    public class PrivateMethodMember : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        public class Target
        {
            private int Method(int i)
            {
                return 0;
            }

            public void CallMethod(int i)
            {
                Method(i);
            }

            private void MethodVoid(int i)
            {
            }

            public void CallMethodVoid(int i)
            {
                MethodVoid(i);
            }
        }

        [TestMethod]
        public void mock_method()
        {
            Mocker.Mock(typeof(Target), "Method", Arg.Any<int>());

            stackFrame = new StackTrace(true).GetFrame(0);
            var target = new Target();
            target.CallMethod(1);
            target.CallMethod(1);
            target.CallMethod(2);
            target.CallMethod(2);

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(typeof(Target), "Method").Args(1)
                    .Once().Called(typeof(Target), "Method").Args(2)
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Expected("Expected to call 1 times, but actually call 2 times", 3),
                Invocation2("hit(1)", 2, "Target::Method(Int32<1>)", 663),
                Invocation2("hit(2)", 2, "Target::Method(Int32<1>)", 663),
                Invocation2("hit(1)", 3, "Target::Method(Int32<2>)", 663),
                Invocation2("hit(2)", 3, "Target::Method(Int32<2>)", 663)
                );
        }

        [TestMethod]
        public void mock_void_method()
        {
            Mocker.MockVoid(typeof(Target), "MethodVoid", Arg.Any<int>());

            stackFrame = new StackTrace(true).GetFrame(0);
            var target = new Target();
            target.CallMethodVoid(1);
            target.CallMethodVoid(1);
            target.CallMethodVoid(2);
            target.CallMethodVoid(2);

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(typeof(Target), "MethodVoid").Args(1)
                    .Once().Called(typeof(Target), "MethodVoid").Args(2)
                    .Verify();
            });

            VerifyMessage(9,
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Expected("Expected to call 1 times, but actually call 2 times", 3),
                Invocation2("hit(1)", 2, "Target::MethodVoid(Int32<1>)", 672),
                Invocation2("hit(2)", 2, "Target::MethodVoid(Int32<1>)", 672),
                Invocation2("hit(1)", 3, "Target::MethodVoid(Int32<2>)", 672),
                Invocation2("hit(2)", 3, "Target::MethodVoid(Int32<2>)", 672)
                );
        }
    }

    [TestClass]
    public class MockPropertyMember : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        public class Target
        {
            public int Property
            {
                get { return 0; }
                set { }
            }
        }

        [TestMethod]
        public void mock_and_verify_getter()
        {
            Mocker.Mock(() => new Target().Property);

            stackFrame = new StackTrace(true).GetFrame(0);
            var target = new Target();
            int i = target.Property;
            int j = target.Property;

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Called(() => new Target().Property)
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 2, "Target::get_Property()", 2),
                Invocation("hit(2)", 2, "Target::get_Property()", 3)
                );

            Verifier
                .Times(2).Called(() => new Target().Property)
                .Verify();
        }

        [TestMethod]
        public void mock_and_verify_setter()
        {
            Mocker.MockVoid(typeof(Target), "Property");

            stackFrame = new StackTrace(true).GetFrame(0);
            var target = new Target();
            target.Property = 1;
            target.Property = 1;

            ExecuteFailed(() =>
            {
                Verifier
                    .Once().Set(typeof(Target), "Property", 1)
                    .Verify();
            });

            VerifyMessage(7,
                Expected("Expected to call 1 times, but actually call 2 times", 2),
                Invocation("hit(1)", 2, "Target::set_Property(Int32<1>)", 2),
                Invocation("hit(2)", 2, "Target::set_Property(Int32<1>)", 3)
                );

            Verifier
                .Times(2).Set(typeof(Target), "Property", 1)
                .Verify();
        }
    }

    [TestClass]
    public class VerifyArgMatchWithSubClassMember : TestBase
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new Target();
            Mocker.When(() => target.Method(Arg.Any<object>())).ThenDefault();
        }

        public class Target
        {
            public void Method(object i)
            {
            }

        }

        [TestMethod]
        public void arg_match_sub_class()
        {
            target.Method(42);

            Verifier.Called(() => target.Method(42)).Verify();
        }

        Target target;
        [TestMethod]
        public void arg_not_match_sub_class_but_value_is_different()
        {
            stackFrame = new StackTrace(true).GetFrame(0);
            target.Method(42);

            ExecuteFailed(() =>
            {
                Verifier.Called(() => target.Method(12306)).Verify();
            });

            VerifyMessage(5,
                Expected("Expected to call at least 1 times, but actually call 0 times", 1),
                Invocation("Target::Method(Object<42>)", 1));

            Verifier.Times(0).Called(() => target.Method(12306)).Verify();
        }

    }

}