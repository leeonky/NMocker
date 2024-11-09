using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;

namespace TestNMocker
{
    [TestClass]
    public class StubPublicStaticPropertyGet
    {
        public class Target
        {
            public static bool called;
            public static int Property
            {
                get
                {
                    called = true;
                    return 100;
                }
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
            Assert.AreEqual(100, Target.Property);
            Assert.IsTrue(Target.called);
            Target.called = false;

            Mocker.When(() => Target.Property).ThenReturn(5);

            Assert.AreEqual(5, Target.Property);
            Assert.IsFalse(Target.called);
        }

        [TestMethod]
        public void reset_stub_after_clear()
        {
            Mocker.When(() => Target.Property).ThenReturn(5);
            Mocker.Clear();

            Assert.AreEqual(100, Target.Property);
            Assert.IsTrue(Target.called);
        }

        [TestMethod]
        public void support_stub_by_lambda()
        {
            Mocker.When(() => Target.Property).Then(args => 999);

            Assert.AreEqual(999, Target.Property);
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => Target.Property).ThenCallActual();

            Assert.AreEqual(100, Target.Property);
            Assert.IsTrue(Target.called);
        }

        [TestMethod]
        public void later_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => Target.Property).ThenReturn(5);
            Mocker.When(() => Target.Property).ThenReturn(10);

            Assert.AreEqual(10, Target.Property);
        }

        [TestMethod]
        public void support_call_default()
        {
            Mocker.When(() => Target.Property).ThenDefault();

            Assert.AreEqual(0, Target.Property);
            Assert.IsFalse(Target.called);
        }
    }

    [TestClass]
    public class StubPublicStaticPropertySet
    {
        public class Target
        {
            public static int value;
            public static int Property
            {
                set
                {
                    Target.value = value;
                }
            }

            public static int Property2
            {
                get
                {
                    return 100;
                }
                set
                {
                    Target.value = value;
                }
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            Target.value = 0;
        }

        [TestMethod]
        public void stub_setter_by_type_and_string()
        {
            Mocker.WhenVoid(typeof(Target), "Property", 1).ThenDefault();
            Mocker.WhenVoid(typeof(Target), "Property", 2).ThenCallActual();

            Target.Property = 1;
            Assert.AreNotEqual(1, Target.value);

            Target.Property = 2;
            Assert.AreEqual(2, Target.value);
        }

        [TestMethod]
        public void stub_getter_keep_setter()
        {
            Mock.When(() => Target.Property2).ThenReturn(10);

            Assert.AreEqual(10, Target.Property2);

            Target.Property2 = 1000;

            Assert.AreEqual(1000, Target.value);
        }

        [TestMethod]
        public void stub_setter_keep_getter()
        {
            Mocker.WhenVoid(typeof(Target), "Property2", 2).ThenDefault();
            Target.Property2 = 2;
            Assert.AreNotEqual(2, Target.value);

            Assert.AreEqual(100, Target.Property2);
        }

        [TestMethod]
        public void stub_setter_and_getter()
        {
            Mock.When(() => Target.Property2).ThenReturn(10);

            Mocker.WhenVoid(typeof(Target), "Property2", 2).ThenDefault();
            Target.Property2 = 2;

            Assert.AreNotEqual(2, Target.value);
            Assert.AreEqual(10, Target.Property2);
        }
    }
}
