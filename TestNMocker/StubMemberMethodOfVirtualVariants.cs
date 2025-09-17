using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

        [TestMethod]
        public void lambda_stub_member_method_with_returned_value_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method").ThenReturn(5);

            Assert.AreEqual(5, dynamicObject.method());
        }

    }

    [TestClass]
    public class StubAbstractMemberMethodWithReturnValue
    {

        public abstract class Target
        {
            public abstract int method();
            protected abstract int protectedMethod();
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

            protected override int protectedMethod()
            {
                called = true;
                return 42;
            }

            public int invokeProtectedMethod()
            {
                return protectedMethod();
            }
        }

        [TestMethod]
        public void method_name_stub_member_method_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "method").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.method());
            Assert.IsFalse(targetImpl.called);
        }

        [TestMethod]
        public void method_name_stub_member_method_with_returned_value_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method").ThenReturn(5);

            Assert.AreEqual(5, dynamicObject.method());
        }

        [TestMethod]
        public void protected_method_name_stub_member_method_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.invokeProtectedMethod());
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

            protected virtual int protectedMethod()
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

            protected override int protectedMethod()
            {
                called = true;
                return 42;
            }

            public int invokeProtectedMethod()
            {
                return protectedMethod();
            }
        }

        [TestMethod]
        public void method_name_stub_member_method_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "method").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.method());
            Assert.IsFalse(targetImpl.called);
        }

        [TestMethod]
        public void method_name_stub_member_method_with_returned_value_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method").ThenReturn(5);

            Assert.AreEqual(5, dynamicObject.method());
        }

        [TestMethod]
        public void protected_method_name_stub_member_method_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.invokeProtectedMethod());
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

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", 1).ThenReturn(5);
            Mocker.When(dynamicObject.GetType(), "method", 2).ThenReturn(10);
            Mocker.When(dynamicObject.GetType(), "method", "hello").ThenReturn(20);
            Mocker.When(dynamicObject.GetType(), "method", "world").ThenReturn(30);

            Assert.AreEqual(5, dynamicObject.method(1));
            Assert.AreEqual(10, dynamicObject.method(2));
            Assert.AreEqual(20, dynamicObject.method("hello"));
            Assert.AreEqual(30, dynamicObject.method("world"));
        }
    }

    [TestClass]
    public class StubAbstractMemberMethodByArg
    {
        public abstract class Target
        {
            public abstract int method(int i);
            public abstract int method(string s);
            protected abstract int protectedMethod(int i);
            protected abstract int protectedMethod(string s);
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

            protected override int protectedMethod(int i)
            {
                called = true;
                return 100;
            }

            protected override int protectedMethod(string s)
            {
                return 200;
            }

            public int invokeProtectedMethod(int i)
            {
                return protectedMethod(i);
            }

            public int invokeProtectedMethod(string s)
            {
                return protectedMethod(s);
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

        [TestMethod]
        public void protected_support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod", 1).ThenReturn(5);
            Mocker.When(typeof(TargetImpl), "protectedMethod", 2).ThenReturn(10);
            Mocker.When(typeof(TargetImpl), "protectedMethod", "hello").ThenReturn(20);
            Mocker.When(typeof(TargetImpl), "protectedMethod", "world").ThenReturn(30);

            Assert.AreEqual(5, target.invokeProtectedMethod(1));
            Assert.AreEqual(10, target.invokeProtectedMethod(2));
            Assert.AreEqual(20, target.invokeProtectedMethod("hello"));
            Assert.AreEqual(30, target.invokeProtectedMethod("world"));
            Assert.AreEqual(100, target.invokeProtectedMethod(100));
            Assert.AreEqual(200, target.invokeProtectedMethod("xxx"));
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", 1).ThenReturn(5);
            Mocker.When(dynamicObject.GetType(), "method", 2).ThenReturn(10);
            Mocker.When(dynamicObject.GetType(), "method", "hello").ThenReturn(20);
            Mocker.When(dynamicObject.GetType(), "method", "world").ThenReturn(30);

            Assert.AreEqual(5, dynamicObject.method(1));
            Assert.AreEqual(10, dynamicObject.method(2));
            Assert.AreEqual(20, dynamicObject.method("hello"));
            Assert.AreEqual(30, dynamicObject.method("world"));
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

            protected virtual int protectedMethod(int i)
            {
                return 300;
            }

            protected virtual int protectedMethod(string s)
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

            protected override int protectedMethod(int i)
            {
                called = true;
                return 100;
            }

            protected override int protectedMethod(string s)
            {
                return 200;
            }

            public int invokeProtectedMethod(int i)
            {
                return protectedMethod(i);
            }

            public int invokeProtectedMethod(string s)
            {
                return protectedMethod(s);
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

        [TestMethod]
        public void protected_support_stub_with_const_arg_and_return_default_when_not_matches_arg()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod", 1).ThenReturn(5);
            Mocker.When(typeof(TargetImpl), "protectedMethod", 2).ThenReturn(10);
            Mocker.When(typeof(TargetImpl), "protectedMethod", "hello").ThenReturn(20);
            Mocker.When(typeof(TargetImpl), "protectedMethod", "world").ThenReturn(30);

            Assert.AreEqual(5, target.invokeProtectedMethod(1));
            Assert.AreEqual(10, target.invokeProtectedMethod(2));
            Assert.AreEqual(20, target.invokeProtectedMethod("hello"));
            Assert.AreEqual(30, target.invokeProtectedMethod("world"));
            Assert.AreEqual(100, target.invokeProtectedMethod(100));
            Assert.AreEqual(200, target.invokeProtectedMethod("xxx"));
        }

        [TestMethod]
        public void support_stub_with_const_arg_and_return_default_when_not_matches_arg_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", 1).ThenReturn(5);
            Mocker.When(dynamicObject.GetType(), "method", 2).ThenReturn(10);
            Mocker.When(dynamicObject.GetType(), "method", "hello").ThenReturn(20);
            Mocker.When(dynamicObject.GetType(), "method", "world").ThenReturn(30);

            Assert.AreEqual(5, dynamicObject.method(1));
            Assert.AreEqual(10, dynamicObject.method(2));
            Assert.AreEqual(20, dynamicObject.method("hello"));
            Assert.AreEqual(30, dynamicObject.method("world"));
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

        [TestMethod]
        public void support_arg_is_match_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, dynamicObject.method(ref i));
        }
    }

    [TestClass]
    public class StubAbstractMemberMethodByRefArg
    {
        public abstract class Target
        {
            public abstract int method(ref int i);
            protected abstract int protectedMethod(ref int i);
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

            protected override int protectedMethod(ref int i)
            {
                i += 100;
                called = true;
                return 100;
            }

            public int invokeProtectedMethod(ref int i)
            {
                return protectedMethod(ref i);
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

        [TestMethod]
        public void protected_support_arg_is_match()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.invokeProtectedMethod(ref i));
            Assert.IsFalse(target.called);
        }

        [TestMethod]
        public void support_arg_is_match_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, dynamicObject.method(ref i));
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

            protected virtual int protectedMethod(ref int i)
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

            protected override int protectedMethod(ref int i)
            {
                i += 100;
                called = true;
                return 100;
            }

            public int invokeProtectedMethod(ref int i)
            {
                return protectedMethod(ref i);
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

        [TestMethod]
        public void protected_support_arg_is_match()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, target.invokeProtectedMethod(ref i));
            Assert.IsFalse(target.called);
        }

        [TestMethod]
        public void support_arg_is_match_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", Arg.Is(1).Ref()).ThenReturn(5);

            int i = 1;
            Assert.AreEqual(5, dynamicObject.method(ref i));
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

        [TestMethod]
        public void support_out_arg_by_lambda_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, dynamicObject.method(out i));
            Assert.AreEqual(1000, i);
        }
    }

    [TestClass]
    public class StubAbstractMemberMethodByOutArg
    {
        public abstract class Target
        {
            public abstract int method(out int i);
            protected abstract int protectedMethod(out int i);
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

            protected override int protectedMethod(out int i)
            {
                i = 1000;
                return 100;
            }

            public int invokeProtectedMethod(out int i)
            {
                return protectedMethod(out i);
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

        [TestMethod]
        public void protected_support_out_arg_by_lambda()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, target.invokeProtectedMethod(out i));
            Assert.AreEqual(1000, i);
        }

        [TestMethod]
        public void support_out_arg_by_lambda_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, dynamicObject.method(out i));
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

            protected virtual int protectedMethod(out int i)
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

            protected override int protectedMethod(out int i)
            {
                i = 1000;
                return 100;
            }

            public int invokeProtectedMethod(out int i)
            {
                return protectedMethod(out i);
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

        [TestMethod]
        public void protected_support_out_arg_by_lambda()
        {
            Mocker.When(typeof(TargetImpl), "protectedMethod", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, target.invokeProtectedMethod(out i));
            Assert.AreEqual(1000, i);
        }

        [TestMethod]
        public void support_out_arg_by_lambda_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.When(dynamicObject.GetType(), "method", Arg.Out<int>()).Then(args =>
            {
                args[0] = 1000;
                return 999;
            });

            int i;
            Assert.AreEqual(999, dynamicObject.method(out i));
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

        [TestMethod]
        public void support_call_actual_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.WhenVoid(dynamicObject.GetType(), "method", 10).ThenCallActual();
            dynamicObject.method(10);
        }
    }

    [TestClass]
    public class StubAbstractVoidMemberMethod
    {
        public abstract class Target
        {
            public abstract void method(int i);
            protected abstract void protectedMethod(int i);
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

            protected override void protectedMethod(int i)
            {
                value = i;
            }

            public void invokeProtectedMethod(int i)
            {
                protectedMethod(i);
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "method", 10).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void protected_support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "protectedMethod", 10).ThenCallActual();
            target.invokeProtectedMethod(10);
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void support_call_actual_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.WhenVoid(dynamicObject.GetType(), "method", 10).ThenCallActual();
            dynamicObject.method(10);
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

            protected virtual void protectedMethod(int i)
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

            protected override void protectedMethod(int i)
            {
                value = i;
            }

            public void invokeProtectedMethod(int i)
            {
                protectedMethod(i);
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "method", 10).ThenCallActual();
            target.method(10);
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void protected_support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "protectedMethod", 10).ThenCallActual();
            target.invokeProtectedMethod(10);
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void support_call_actual_of_dynamic_object()
        {
            var dynamicObject = new Mock<Target>().Object;

            Mocker.WhenVoid(dynamicObject.GetType(), "method", 10).ThenCallActual();
            dynamicObject.method(10);
        }
    }
}
