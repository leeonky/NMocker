using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;

namespace TestNMocker
{
    [TestClass]
    public class StubPublicStaticProperty
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
}
