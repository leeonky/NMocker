using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;

namespace TestNMocker
{
    [TestClass]
    public class StubPrivateMemberMethodWithReturnValue
    {
        public class Target
        {
            public bool called;

            private int privateMethod(int i)
            {
                called = true;
                return 200;
            }

            private int privateMethod(string s)
            {
                called = true;
                return 300;
            }

            public int invokePrivateMethodInt()
            {
                return privateMethod(1);
            }

            public int invokePrivateMethodString()
            {
                return privateMethod("1");
            }

            private void privateVoid(ref int i)
            {
            }

            public void invokePrivateIntVoid(ref int i)
            {
                privateVoid(ref i);
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
        public void support_stub_private_method()
        {
            Mocker.When(typeof(Target), "privateMethod", Arg.Any<int>()).ThenReturn(1);

            Assert.AreEqual(1, target.invokePrivateMethodInt());
        }

        [TestMethod]
        public void user_raw_arg_value_in()
        {
            Mocker.When(typeof(Target), "privateMethod", 1).ThenReturn(1);

            Assert.AreEqual(1, target.invokePrivateMethodInt());
        }

        [TestMethod]
        public void support_stub_private_void_method()
        {
            Mocker.WhenVoid(typeof(Target), "privateVoid", Arg.Is(10).Ref(1)).ThenDefault();

            int i = 10;
            target.invokePrivateIntVoid(ref i);

            Assert.AreEqual(1, i);
        }
    }

}