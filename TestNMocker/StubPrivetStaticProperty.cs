using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;

namespace TestNMocker
{
    [TestClass]
    public class StubPrivateStaticProperty
    {
        public class Target
        {
            private static int PrivateProperty
            {
                get { return 100; }
            }

            public static int invokePrivate()
            {
                return PrivateProperty;
            }
        }

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        [TestMethod]
        public void user_raw_arg_value_in()
        {
            Mocker.When(typeof(Target), "PrivateProperty").ThenReturn(1);

            Assert.AreEqual(1, Target.invokePrivate());
        }
    }
}
