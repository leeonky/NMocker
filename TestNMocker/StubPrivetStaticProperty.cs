using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;

namespace TestNMocker
{
    [TestClass]
    public class StubPrivateStaticProperty
    {
        public class Target
        {
            public static int value;
            private static int PrivateProperty
            {
                get { return 100; }
                set { Target.value = value; }
            }

            public static int invokePrivate()
            {
                return PrivateProperty;
            }

            public static void invokePrivate(int i)
            {
                PrivateProperty = i;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Target.value = 0;
            Mocker.Clear();
        }

        [TestMethod]
        public void stub_with_raw_arg_value()
        {
            Mocker.When(typeof(Target), "PrivateProperty").ThenReturn(1);

            Assert.AreEqual(1, Target.invokePrivate());
        }

        [TestMethod]
        public void stub_setter()
        {
            Mocker.WhenVoid(typeof(Target), "PrivateProperty", 1).ThenDefault();

            Target.invokePrivate(1);

            Assert.AreNotEqual(1, Target.value);
        }
    }
}
