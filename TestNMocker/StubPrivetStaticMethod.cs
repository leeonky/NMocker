using Microsoft.VisualStudio.TestTools.UnitTesting;
using nmocker;

namespace TestNMocker
{
    [TestClass]
    public class StubPrivateStaticMethod
    {
        public class Target
        {
            private static int privateStatic(int i)
            {
                return 100;
            }
            private static int privateStatic(string i)
            {
                return 200;
            }

            public static int invokePrivateStaticInt()
            {
                return privateStatic(1);
            }
            public static int invokePrivateStaticString()
            {
                return privateStatic("1");
            }
        }


        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        [TestMethod]
        public void support_stub_private_method()
        {
            Mocker.When(typeof(Target), "privateStatic", Arg<int>.Any()).ThenReturn(1);

            Assert.AreEqual(1, Target.invokePrivateStaticInt());
        }

        [TestMethod]
        public void user_raw_arg_value_in()
        {
            Mocker.When(typeof(Target), "privateStatic", 1).ThenReturn(1);

            Assert.AreEqual(1, Target.invokePrivateStaticInt());
        }
    }
}
