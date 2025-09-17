using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;

namespace TestNMocker
{
    [TestClass]
    public class StubPrivateMemberPropertyOfNonVirtualClass
    {
        public class Target
        {
            public int value;
            private int PrivateProperty
            {
                get { return 100; }
                set { this.value = value; }
            }

            public int invokePrivate()
            {
                return PrivateProperty;
            }

            public void invokePrivate(int i)
            {
                PrivateProperty = i;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        [TestMethod]
        public void stub_with_raw_arg_value()
        {
            var target = new Target();
            target.value = 0;
            
            Mocker.When(typeof(Target), "PrivateProperty").ThenReturn(1);

            Assert.AreEqual(1, target.invokePrivate());
        }

        [TestMethod]
        public void stub_setter()
        {
            var target = new Target();
            target.value = 0;
            
            Mocker.WhenVoid(typeof(Target), "PrivateProperty", 1).ThenDefault();

            target.invokePrivate(1);

            Assert.AreNotEqual(1, target.value);
        }
    }
}