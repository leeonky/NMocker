﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            public static int method1(int i)
            {
                return 100;
            }
        }

        [TestMethod]
        public void ReplaceMethodByReturnValue()
        {
            Target.called = false;

            Mocker.When(()=> Target.method()).ThenReturn(5);

            Assert.AreEqual(5, Target.method());
            Assert.IsFalse(Target.called);
        }

        [TestMethod]
        public void ReplaceMethodByReturnValueAndArgs()
        {
            Target.called = false;

            Mocker.When(()=> Target.method1(1)).ThenReturn(5);
            Mocker.When(()=> Target.method1(2)).ThenReturn(10);

            Assert.AreEqual(5, Target.method1(1));
            Assert.AreEqual(10, Target.method1(2));
        }
    }
}
