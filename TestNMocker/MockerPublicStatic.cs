using Microsoft.VisualStudio.TestTools.UnitTesting;
using nmocker;

namespace mockerTest
{
    [TestClass]
    public partial class MockerPublicStaticMethod
    {
        public class Target
        {
            public static bool called;
            public static int method()
            {
                called = true;
                return 100;
            }
        }

        [TestMethod]
        public void ReplaceMethodByReturnValue()
        {
            Target.called = false;

            Mocker.When(typeof(Target), "method").ThenReturn(5);

            Assert.AreEqual(5, Target.method());
            Assert.IsFalse(Target.called);
        }
    }
}
