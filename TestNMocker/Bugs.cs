using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;

namespace TestNMocker
{
    [TestClass]
    public class Bugs
    {
        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
        }

        public interface IObject
        {
            void action();
        }

        public class DefaultObject : IObject
        {
            public void action() { }
        }

        public class Target
        {
            public static int method(object arg)
            {
                return 1;
            }

            public static int method2(IObject arg)
            {
                return 1;
            }
        }

        [TestMethod]
        public void stub_with_any_obj_arg_implicit_not_work()
        {
            Mocker.When(() => Target.method(Arg.Any<object>())).ThenReturn(100);

            Assert.AreEqual(100, Target.method(new object()));
        }

        [TestMethod]
        public void stub_with_particular_obj_arg()
        {
            object obj = new object();
            Mocker.When(() => Target.method(Arg.Is(obj))).ThenReturn(100);

            Assert.AreEqual(100, Target.method(obj));
            Assert.AreEqual(1, Target.method(new object()));

            Mocker.When(() => Target.method(obj)).ThenReturn(200);
            Assert.AreEqual(200, Target.method(obj));
        }

        [TestMethod]
        public void stub_with_interface()
        {
            Mocker.When(() => Target.method(Arg.Any<IObject>())).ThenReturn(100);

            Assert.AreEqual(100, Target.method(new DefaultObject()));
        }

        [TestMethod]
        public void stub_with_sub_class()
        {
            Mocker.When(() => Target.method(Arg.Any<DefaultObject>())).ThenReturn(100);

            Assert.AreEqual(100, Target.method(new DefaultObject()));
        }


        [TestMethod]
        public void stub_with_particular_instance_arg()
        {
            DefaultObject d = new DefaultObject();
            Mocker.When(() => Target.method(Arg.Is(d))).ThenReturn(100);

            Assert.AreEqual(100, Target.method(d));
            Assert.AreEqual(1, Target.method(new DefaultObject()));

            Mocker.When(() => Target.method(d)).ThenReturn(200);
            Assert.AreEqual(200, Target.method(d));
        }
    }
}
