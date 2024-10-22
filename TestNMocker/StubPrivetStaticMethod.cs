using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;

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

            private static void privateStaticVoid(ref int i)
            {
            }

            public static void invokePrivateStaticIntVoid(ref int i)
            {
                privateStaticVoid(ref i);
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
            Mocker.When(typeof(Target), "privateStatic", Arg.Any<int>()).ThenReturn(1);

            Assert.AreEqual(1, Target.invokePrivateStaticInt());
        }

        [TestMethod]
        public void user_raw_arg_value_in()
        {
            Mocker.When(typeof(Target), "privateStatic", 1).ThenReturn(1);

            Assert.AreEqual(1, Target.invokePrivateStaticInt());
        }

        [TestMethod]
        public void support_stub_private_void_method()
        {
            Mocker.WhenVoid(typeof(Target), "privateStaticVoid", Arg.Is(10).Ref(1)).ThenDefault();

            int i = 10;
            Target.invokePrivateStaticIntVoid(ref i);

            Assert.AreEqual(1, i);
        }
    }
}
