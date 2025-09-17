using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMocker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNMocker
{
    [TestClass]
    public class StubMemberMethodOfInterfaceWithReturnValue
    {

        public interface Target
        {
            int method();
        }

        private TargetImpl targetImpl;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            targetImpl = new TargetImpl();
        }

        public class TargetImpl : Target
        {

            public bool called;
            public int method()
            {
                called = true;
                return 42;
            }
        }

        [TestMethod]
        public void lambda_stub_member_method_with_returned_value()
        {
            Mocker.When(() => targetImpl.method()).ThenReturn(5);

            Assert.AreEqual(5, targetImpl.method());
            Assert.IsFalse(targetImpl.called);
        }

    }

    [TestClass]
    public class StubAbstractMemberMethodWithReturnValue
    {

        public abstract class Target
        {
            public abstract int method();
        }

        private TargetImpl targetImpl;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            targetImpl = new TargetImpl();
        }

        public class TargetImpl : Target
        {

            public bool called;

            public override int method()
            {
                called = true;
                return 42;
            }
        }

        [TestMethod]
        public void method_name_stub_member_method_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "method").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.method());
            Assert.IsFalse(targetImpl.called);
        }

    }

    [TestClass]
    public class StubVirtualMemberMethodWithReturnValue
    {

        public class Target
        {
            public virtual int method()
            {
                return 12306;
            }
        }

        private TargetImpl targetImpl;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            targetImpl = new TargetImpl();
        }

        public class TargetImpl : Target
        {

            public bool called;

            public override int method()
            {
                called = true;
                return 42;
            }
        }

        [TestMethod]
        public void method_name_stub_member_method_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "method").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.method());
            Assert.IsFalse(targetImpl.called);
        }

    }

    [TestClass]
    public class StubInterfaceMemberMethodByArg
    {
        public interface Target
        {
            int method(int i);
            int method(string s);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.called = false;
        }

        public class TargetImpl : Target
        {
            public bool called;

            public int method(int i)
            {
                called = true;
                return 100;
            }

            public int method(string s)
            {
                return 200;
            }
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(() => target.method(1)).ThenReturn(5);
            Mocker.When(() => target.method(2)).ThenReturn(10);
            Mocker.When(() => target.method("hello")).ThenReturn(20);
            Mocker.When(() => target.method("world")).ThenReturn(30);

            Assert.AreEqual(5, target.method(1));
            Assert.AreEqual(10, target.method(2));
            Assert.AreEqual(20, target.method("hello"));
            Assert.AreEqual(30, target.method("world"));
            Assert.AreEqual(100, target.method(100));
            Assert.AreEqual(200, target.method("xxx"));
        }
    }

    [TestClass]
    public class StubAbstractMemberMethodByArg
    {
        public abstract class Target
        {
            public abstract int method(int i);
            public abstract int method(string s);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.called = false;
        }

        public class TargetImpl : Target
        {
            public bool called;

            public override int method(int i)
            {
                called = true;
                return 100;
            }

            public override int method(string s)
            {
                return 200;
            }
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(typeof(TargetImpl), "method", 1).ThenReturn(5);
            Mocker.When(typeof(TargetImpl), "method", 2).ThenReturn(10);
            Mocker.When(typeof(TargetImpl), "method", "hello").ThenReturn(20);
            Mocker.When(typeof(TargetImpl), "method", "world").ThenReturn(30);

            Assert.AreEqual(5, target.method(1));
            Assert.AreEqual(10, target.method(2));
            Assert.AreEqual(20, target.method("hello"));
            Assert.AreEqual(30, target.method("world"));
            Assert.AreEqual(100, target.method(100));
            Assert.AreEqual(200, target.method("xxx"));
        }
    }

    [TestClass]
    public class StubVirtualMemberMethodByArg
    {
        public class Target
        {
            public virtual int method(int i)
            {
                return 300;
            }

            public virtual int method(string s)
            {
                return 400;
            }
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.called = false;
        }

        public class TargetImpl : Target
        {
            public bool called;

            public override int method(int i)
            {
                called = true;
                return 100;
            }

            public override int method(string s)
            {
                return 200;
            }
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(typeof(TargetImpl), "method", 1).ThenReturn(5);
            Mocker.When(typeof(TargetImpl), "method", 2).ThenReturn(10);
            Mocker.When(typeof(TargetImpl), "method", "hello").ThenReturn(20);
            Mocker.When(typeof(TargetImpl), "method", "world").ThenReturn(30);

            Assert.AreEqual(5, target.method(1));
            Assert.AreEqual(10, target.method(2));
            Assert.AreEqual(20, target.method("hello"));
            Assert.AreEqual(30, target.method("world"));
            Assert.AreEqual(100, target.method(100));
            Assert.AreEqual(200, target.method("xxx"));
        }
    }

    [TestClass]
    public class StubInterfaceMemberMethodByRefArg
    {
        public interface Target
        {
            int method(ref int i);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.called = false;
        }

        public class TargetImpl : Target
        {
            public bool called;
            public int method(ref int i)
            {
                i += 100;
                called = true;
                return 100;
            }
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(typeof(TargetImpl), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.method(ref i));
            Assert.IsFalse(target.called);
        }
    }

    [TestClass]
    public class StubAbstractMemberMethodByRefArg
    {
        public abstract class Target
        {
            public abstract int method(ref int i);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.called = false;
        }

        public class TargetImpl : Target
        {
            public bool called;
            public override int method(ref int i)
            {
                i += 100;
                called = true;
                return 100;
            }
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(typeof(TargetImpl), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.method(ref i));
            Assert.IsFalse(target.called);
        }
    }

    [TestClass]
    public class StubVirtualMemberMethodByRefArg
    {
        public class Target
        {
            public virtual int method(ref int i)
            {
                i += 200;
                return 300;
            }
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.called = false;
        }

        public class TargetImpl : Target
        {
            public bool called;
            public override int method(ref int i)
            {
                i += 100;
                called = true;
                return 100;
            }
        }

        [TestMethod]
        public void support_arg_is_match()
        {
            Mocker.When(typeof(TargetImpl), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.method(ref i));
            Assert.IsFalse(target.called);
        }
    }

    [TestClass]
    public class StubInterfaceMemberMethodByOutArg
    {
        public interface Target
        {
            int method(out int i);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
        }

        public class TargetImpl : Target
        {
            public int method(out int i)
            {
                i = 1000;
                return 100;
            }
        }

        [TestMethod]
        public void support_out_arg_by_lambda()
        {
            Mocker.When(typeof(TargetImpl), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, target.method(out i));
            Assert.AreEqual(1000, i);
        }
    }

    [TestClass]
    public class StubAbstractMemberMethodByOutArg
    {
        public abstract class Target
        {
            public abstract int method(out int i);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
        }

        public class TargetImpl : Target
        {
            public override int method(out int i)
            {
                i = 1000;
                return 100;
            }
        }

        [TestMethod]
        public void support_out_arg_by_lambda()
        {
            Mocker.When(typeof(TargetImpl), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, target.method(out i));
            Assert.AreEqual(1000, i);
        }
    }

    [TestClass]
    public class StubVirtualMemberMethodByOutArg
    {
        public class Target
        {
            public virtual int method(out int i)
            {
                i = 2000;
                return 200;
            }
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
        }

        public class TargetImpl : Target
        {
            public override int method(out int i)
            {
                i = 1000;
                return 100;
            }
        }

        [TestMethod]
        public void support_out_arg_by_lambda()
        {
            Mocker.When(typeof(TargetImpl), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, target.method(out i));
            Assert.AreEqual(1000, i);
        }
    }

    [TestClass]
    public class StubInterfaceVoidMemberMethod
    {
        public interface Target
        {
            void method(int i);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.value = 0;
        }

        public class TargetImpl : Target
        {
            public int value;

            public void method(int i)
            {
                value = i;
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.When(() => target.method(10)).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }
    }

    [TestClass]
    public class StubAbstractVoidMemberMethod
    {
        public abstract class Target
        {
            public abstract void method(int i);
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.value = 0;
        }

        public class TargetImpl : Target
        {
            public int value;

            public override void method(int i)
            {
                value = i;
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "method", 10).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }
    }

    [TestClass]
    public class StubVirtualVoidMemberMethod
    {
        public class Target
        {
            public virtual void method(int i)
            {
                // Base implementation does nothing
            }
        }

        private TargetImpl target;

        [TestInitialize]
        public void setup()
        {
            Mocker.Clear();
            target = new TargetImpl();
            target.value = 0;
        }

        public class TargetImpl : Target
        {
            public int value;

            public override void method(int i)
            {
                value = i;
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "method", 10).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }
    }
}
