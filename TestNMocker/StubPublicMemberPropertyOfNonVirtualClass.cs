using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;

namespace TestNMocker
{
    [TestClass]
    public class StubPublicMemberPropertyGet
    {
        public class Target
        {
            public bool called;

            public int Property
            {
                get
                {
                    called = true;
                    return 100;
                }
            }

            public int invoke_property()
            {
                int res = Property;
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
        public void stub_member_property_with_returned_value()
        {
            Assert.AreEqual(100, target.Property);
            Assert.IsTrue(target.called);
            target.called = false;

            Mocker.When(() => target.Property).ThenReturn(5);

            Assert.AreEqual(5, target.Property);
            Assert.IsFalse(target.called);
        }

        [TestMethod]
        public void reset_stub_after_clear()
        {
            Mocker.When(() => target.Property).ThenReturn(5);
            Mocker.Clear();

            Assert.AreEqual(100, target.Property);
            Assert.IsTrue(target.called);
        }

        [TestMethod]
        public void support_stub_by_lambda()
        {
            Mocker.When(() => target.Property).Then(args => 999);

            Assert.AreEqual(999, target.Property);
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => target.Property).ThenCallActual();

            Assert.AreEqual(100, target.Property);
            Assert.IsTrue(target.called);
        }

        [TestMethod]
        public void later_stub_will_overwrite_the_earlier()
        {
            Mocker.When(() => target.Property).ThenReturn(5);
            Mocker.When(() => target.Property).ThenReturn(10);

            Assert.AreEqual(10, target.Property);
        }

        [TestMethod]
        public void support_call_default()
        {
            Mocker.When(() => target.Property).ThenDefault();

            Assert.AreEqual(0, target.Property);
            Assert.IsFalse(target.called);
        }
    }

    [TestClass]
    public class StubPublicMemberPropertySet
    {
        public class Target
        {
            public int value;

            public int Property
            {
                set
                {
                    this.value = value;
                }
            }

            public int Property2
            {
                get
                {
                    return 100;
                }
                set
                {
                    this.value = value;
                }
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
        public void stub_setter_by_type_and_string()
        {
            Mocker.WhenVoid(typeof(Target), "Property", 1).ThenDefault();
            Mocker.WhenVoid(typeof(Target), "Property", 2).ThenCallActual();

            target.Property = 1;
            Assert.AreNotEqual(1, target.value);

            target.Property = 2;
            Assert.AreEqual(2, target.value);
        }

        [TestMethod]
        public void stub_getter_keep_setter()
        {
            Mocker.When(() => target.Property2).ThenReturn(10);

            Assert.AreEqual(10, target.Property2);

            target.Property2 = 1000;

            Assert.AreEqual(1000, target.value);
        }

        [TestMethod]
        public void stub_setter_keep_getter()
        {
            Mocker.WhenVoid(typeof(Target), "Property2", 2).ThenDefault();
            target.Property2 = 2;
            Assert.AreNotEqual(2, target.value);

            Assert.AreEqual(100, target.Property2);
        }

        [TestMethod]
        public void stub_setter_and_getter()
        {
            Mocker.When(() => target.Property2).ThenReturn(10);

            Mocker.WhenVoid(typeof(Target), "Property2", 2).ThenDefault();
            target.Property2 = 2;

            Assert.AreNotEqual(2, target.value);
            Assert.AreEqual(10, target.Property2);
        }

    }
}