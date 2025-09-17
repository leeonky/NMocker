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
    public class StubMemberPropertyOfInterfaceWithReturnValue
    {
        public interface Target
        {
            int Property { get; }
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
            public int Property
            {
                get
                {
                    called = true;
                    return 42;
                }
            }
        }

        [TestMethod]
        public void lambda_stub_member_property_with_returned_value()
        {
            Mocker.When(() => targetImpl.Property).ThenReturn(5);

            Assert.AreEqual(5, targetImpl.Property);
            Assert.IsFalse(targetImpl.called);
        }
    }

    [TestClass]
    public class StubAbstractMemberPropertyWithReturnValue
    {
        public abstract class Target
        {
            public abstract int Property { get; }
            protected abstract int ProtectedProperty { get; }
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

            public override int Property
            {
                get
                {
                    called = true;
                    return 42;
                }
            }

            protected override int ProtectedProperty
            {
                get
                {
                    called = true;
                    return 42;
                }
            }

            public int invokeProtectedProperty()
            {
                return ProtectedProperty;
            }
        }

        [TestMethod]
        public void property_name_stub_member_property_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "Property").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.Property);
            Assert.IsFalse(targetImpl.called);
        }

        [TestMethod]
        public void protected_property_name_stub_member_property_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "ProtectedProperty").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.invokeProtectedProperty());
            Assert.IsFalse(targetImpl.called);
        }
    }

    [TestClass]
    public class StubVirtualMemberPropertyWithReturnValue
    {
        public class Target
        {
            public virtual int Property
            {
                get
                {
                    return 12306;
                }
            }

            protected virtual int ProtectedProperty
            {
                get
                {
                    return 12306;
                }
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

            public override int Property
            {
                get
                {
                    called = true;
                    return 42;
                }
            }

            protected override int ProtectedProperty
            {
                get
                {
                    called = true;
                    return 42;
                }
            }

            public int invokeProtectedProperty()
            {
                return ProtectedProperty;
            }
        }

        [TestMethod]
        public void property_name_stub_member_property_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "Property").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.Property);
            Assert.IsFalse(targetImpl.called);
        }

        [TestMethod]
        public void protected_property_name_stub_member_property_with_returned_value()
        {
            Mocker.When(typeof(TargetImpl), "ProtectedProperty").ThenReturn(5);

            Assert.AreEqual(5, targetImpl.invokeProtectedProperty());
            Assert.IsFalse(targetImpl.called);
        }
    }

    [TestClass]
    public class StubInterfaceVoidMemberPropertySet
    {
        public interface Target
        {
            int Property { set; }
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

            public int Property
            {
                set
                {
                    this.value = value;
                }
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "Property", 10).ThenCallActual();
            target.Property = 10;
            Assert.AreEqual(10, target.value);
        }
    }

    [TestClass]
    public class StubAbstractVoidMemberPropertySet
    {
        public abstract class Target
        {
            public abstract int Property { set; }
            protected abstract int ProtectedProperty { set; }
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

            public override int Property
            {
                set
                {
                    this.value = value;
                }
            }

            protected override int ProtectedProperty
            {
                set
                {
                    this.value = value;
                }
            }

            public void invokeProtectedPropertySetter(int value)
            {
                ProtectedProperty = value;
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "Property", 10).ThenCallActual();
            target.Property = 10;
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void protected_support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "ProtectedProperty", 10).ThenCallActual();
            target.invokeProtectedPropertySetter(10);
            Assert.AreEqual(10, target.value);
        }
    }

    [TestClass]
    public class StubVirtualVoidMemberPropertySet
    {
        public class Target
        {
            public virtual int Property
            {
                set
                {
                    // Base implementation does nothing
                }
            }

            protected virtual int ProtectedProperty
            {
                set
                {
                    // Base implementation does nothing
                }
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

            public override int Property
            {
                set
                {
                    this.value = value;
                }
            }

            protected override int ProtectedProperty
            {
                set
                {
                    this.value = value;
                }
            }

            public void invokeProtectedPropertySetter(int value)
            {
                ProtectedProperty = value;
            }
        }

        [TestMethod]
        public void support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "Property", 10).ThenCallActual();
            target.Property = 10;
            Assert.AreEqual(10, target.value);
        }

        [TestMethod]
        public void protected_support_call_actual()
        {
            Mocker.WhenVoid(typeof(TargetImpl), "ProtectedProperty", 10).ThenCallActual();
            target.invokeProtectedPropertySetter(10);
            Assert.AreEqual(10, target.value);
        }
    }
}