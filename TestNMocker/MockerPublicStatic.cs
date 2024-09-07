using Microsoft.VisualStudio.TestTools.UnitTesting;
using nmocker;

namespace mockerTest
{
    [TestClass]
    public class MockerPublicStaticMethod
    {
        public class Target
        {
            public static bool called;
            public static int method()
            {
                called = true;
                return 100;
            }

            public static int method1(int i)
            {
                return 100;
            }
        }


        [TestInitialize]
        public void setup()
        {
            Mocker.clear();
        }

        [TestMethod]
        public void stub_static_method_with_returned_value()
        {
            Assert.AreEqual(100, Target.method());
            Assert.IsTrue(Target.called);

            Target.called = false;

            Mocker.When(()=> Target.method()).ThenReturn(5);

            Assert.AreEqual(5, Target.method());
            Assert.IsFalse(Target.called);
        }

        [TestMethod]
        public void reset_stub_after_clear()
        {
            Mocker.When(()=> Target.method()).ThenReturn(5);
            Mocker.clear();

            Assert.AreEqual(100, Target.method());
            Assert.IsTrue(Target.called);
        }

        [TestMethod]
        public void stub_static_method_with_returned_value_and_args()
        {
            Target.called = false;

            Mocker.When(()=> Target.method1(1)).ThenReturn(5);
            Mocker.When(()=> Target.method1(2)).ThenReturn(10);

            Assert.AreEqual(5, Target.method1(1));
            Assert.AreEqual(10, Target.method1(2));
        }

        [TestMethod]
        public void override_privours_stub()
        {
            Target.called = false;

            Mocker.When(()=> Target.method1(1)).ThenReturn(5);
            Mocker.When(()=> Target.method1(1)).ThenReturn(10);

            Assert.AreEqual(10, Target.method1(1));
        }
    }
}
