# NMocker 使用文档

## 简介

NMocker 是一个为 C# 设计的静态方法 Mocking/Stubbing 框架，使用 Harmony 库进行运行时方法修补。它能够精确控制静态方法的行为，无需依赖注入或设计更改即可进行单元测试。

## 核心特性

- 🎯 **静态方法拦截** - 支持 public/private 静态方法和属性
- 🏛️ **实例方法拦截** - 支持实例方法、虚拟方法、抽象方法、接口方法
- 🔧 **灵活的参数匹配** - 精确值、类型匹配、自定义谓词
- 📊 **调用验证** - 完整的调用次数和参数验证
- 🎭 **多种 Stub 行为** - 返回值、Lambda 表达式、调用原方法
- 🏷️ **ref/out 参数支持** - 完整的引用参数处理

## 基本用法

### 1. 测试初始化

在每个测试之前始终清除现有的 Mock，确保干净的测试环境：

```csharp
[TestInitialize]
public void Setup()
{
    Mocker.Clear();
}
```

### 2. Stub 静态方法

使用流畅的 API 对静态方法进行 Stub：

```csharp
// 基本 Stub
Mocker.When(() => Target.Method()).ThenReturn(5);
Assert.AreEqual(5, Target.Method());

// Stub 带参数的方法
Mocker.When(() => Target.Method(1)).ThenReturn(10);
Assert.AreEqual(10, Target.Method(1));
```

### 3. Stub 实例方法

NMocker 同样支持对实例方法进行 Stub：

```csharp
var target = new Target();

// 基本实例方法 Stub
Mocker.When(() => target.Method()).ThenReturn(5);
Assert.AreEqual(5, target.Method());

// 带参数的实例方法 Stub
Mocker.When(() => target.Method(1)).ThenReturn(10);
Assert.AreEqual(10, target.Method(1));
```

## Stub 行为

### 1. 返回固定值
```csharp
Mocker.When(() => Target.Method()).ThenReturn(5);
```

### 2. 使用 Lambda 表达式
```csharp
Mocker.When(() => Target.Method()).Then(args => 999);

// 使用传入的参数
Mocker.When(() => Target.Method(Arg.Any<int>())).Then(args => ((int)args[0]) + 1);
```

### 3. 调用原方法
```csharp
Mocker.When(() => Target.Method()).ThenCallActual();
```

### 4. 返回默认值
```csharp
Mocker.When(() => Target.Method()).ThenDefault(); // 返回 0 (int 的默认值)
```

## 参数匹配

### 1. 精确值匹配
```csharp
Mocker.When(() => Target.Method(1)).ThenReturn(5);
Mocker.When(() => Target.Method(2)).ThenReturn(10);

// 使用变量
int i = 1;
Mocker.When(() => Target.Method(i)).ThenReturn(5);
```

### 2. Arg.Is() 显式匹配
```csharp
Mocker.When(() => Target.Method(Arg.Is(1))).ThenReturn(5);
```

### 3. Arg.Any<T>() 类型匹配
```csharp
Mocker.When(() => Target.Method(Arg.Any<int>())).ThenReturn(5);
```

### 4. Arg.That<T>() 自定义谓词匹配
```csharp
Mocker.When(() => Target.Method(Arg.That<int>(i => i > 5))).ThenReturn(5);
```

### 5. 处理 null 值
```csharp
Mocker.When(() => Target.Method(null)).ThenReturn(1);
```

## ref/out 参数支持

### ref 参数
```csharp
// 基本匹配
Mocker.When(typeof(Target), "Method", Arg.Is(1).Ref()).ThenReturn(5);

// 在 Lambda 中修改 ref 值
Mocker.When(typeof(Target), "Method", Arg.Is(1).Ref()).Then(args => args[0] = 999);

// 直接设置 ref 值
Mocker.When(typeof(Target), "Method", Arg.Is(10).Ref(1000)).ThenDefault();
```

### out 参数
```csharp
// 在 Lambda 中设置 out 值
Mocker.When(typeof(Target), "Method", Arg.Out<int>()).Then(args =>
{
    args[0] = 1000;
    return 999;
});

// 直接设置 out 值
Mocker.When(typeof(Target), "Method", Arg.Out(1000)).ThenDefault();
```

## 属性 Stub

### 静态属性

#### Getter 属性
```csharp
Mocker.When(() => Target.Property).ThenReturn(5);
```

#### Setter 属性
```csharp
// 使用类型和字符串方式
Mocker.WhenVoid(typeof(Target), "Property", 1).ThenDefault();
```

### 实例属性

#### Getter 属性
```csharp
var target = new Target();
Mocker.When(() => target.Property).ThenReturn(5);
Assert.AreEqual(5, target.Property);
```

#### Setter 属性
```csharp
// 使用类型和字符串方式
Mocker.WhenVoid(typeof(Target), "Property", 1).ThenDefault();

// 或者对特定实例
target.Property = 1; // 不会执行实际的 setter
```

#### 独立控制 Getter 和 Setter
```csharp
var target = new Target();

// 只 Stub getter，保持 setter 正常工作
Mocker.When(() => target.Property).ThenReturn(10);
target.Property = 1000; // setter 正常执行
Assert.AreEqual(10, target.Property); // getter 返回 Stub 值

// 只 Stub setter，保持 getter 正常工作
Mocker.WhenVoid(typeof(Target), "Property", 2).ThenDefault();
target.Property = 2; // setter 被拦截，不执行
Assert.AreEqual(100, target.Property); // getter 返回原值
```

## void 方法处理

### 静态 void 方法
```csharp
Mocker.When(() => Target.VoidMethod(10)).ThenCallActual();
Mocker.When(() => Target.VoidMethod(10)).ThenDefault();
Mocker.When(() => Target.VoidMethod(10)).Then(args => { /* 自定义逻辑 */ });
```

### 实例 void 方法
```csharp
var target = new Target();
Mocker.When(() => target.VoidMethod(10)).ThenCallActual();
Mocker.When(() => target.VoidMethod(10)).ThenDefault();
Mocker.When(() => target.VoidMethod(10)).Then(args => { /* 自定义逻辑 */ });
```

### 使用类型和方法名
```csharp
Mocker.WhenVoid(typeof(Target), "VoidMethod", 10).Then(args => { /* 自定义逻辑 */ });
```

## 私有方法 Stub

对于私有静态方法和实例方法，使用类型和方法名的方式：

### 私有静态方法
```csharp
// 私有静态方法
Mocker.When(typeof(Target), "PrivateStaticMethod", Arg.Any<int>()).ThenReturn(1);

// 私有静态 void 方法
Mocker.WhenVoid(typeof(Target), "PrivateStaticVoidMethod", Arg.Is(10).Ref(1)).ThenDefault();
```

### 私有实例方法
```csharp
// 私有实例方法
Mocker.When(typeof(Target), "PrivateMethod", Arg.Any<int>()).ThenReturn(1);

// 私有实例 void 方法
Mocker.WhenVoid(typeof(Target), "PrivateVoidMethod", Arg.Is(10).Ref(1)).ThenDefault();
```

### 私有属性
```csharp
// 私有属性 getter
Mocker.When(typeof(Target), "PrivateProperty").ThenReturn(1);

// 私有属性 setter
Mocker.WhenVoid(typeof(Target), "PrivateProperty", 1).ThenDefault();
```

## Mock 和验证

### 1. 启用 Mock 模式
```csharp
// 静态方法 Mock
Mocker.Mock(() => Target.Method(Arg.Any<string>()));

// 实例方法 Mock
var target = new Target();
Mocker.Mock(() => target.Method(Arg.Any<string>()));
```

### 2. 验证调用次数

#### 基本验证
```csharp
// 验证静态方法调用次数
Verifier.Times(1).Called(() => Target.Method("a")).Verify();

// 验证实例方法调用次数
var target = new Target();
Verifier.Times(1).Called(() => target.Method("a")).Verify();

// 验证至少调用次数
Verifier.AtLeast(3).Called(() => Target.Method("a")).Verify();

// 验证最多调用次数
Verifier.AtMost(1).Called(() => target.Method("a")).Verify();

// 验证调用一次
Verifier.Once().Called(() => Target.Method("a")).Verify();
```

#### 链式验证
```csharp
Verifier
    .Times(1).Called(() => Target.Method("a"))
    .Times(2).Called(() => Target.Method("b"))
    .Verify();
```

#### 私有方法验证
```csharp
Mocker.Mock(typeof(Target), "PrivateMethod", Arg.Any<int>());
Verifier.Once().Called(typeof(Target), "PrivateMethod").Args(1).Verify();
```

#### 属性验证
```csharp
// 静态属性 Getter 验证
Mocker.Mock(() => Target.Property);
Verifier.Times(2).Called(() => Target.Property).Verify();

// 实例属性 Getter 验证
var target = new Target();
Mocker.Mock(() => target.Property);
Verifier.Times(2).Called(() => target.Property).Verify();

// 静态属性 Setter 验证
Mocker.MockVoid(typeof(Target), "Property");
Verifier.Times(2).Set(typeof(Target), "Property", 1).Verify();

// 实例属性 Setter 验证 (使用同样的方式)
Mocker.MockVoid(typeof(Target), "Property");
Verifier.Times(2).Set(typeof(Target), "Property", 1).Verify();
```

## 虚拟方法、抽象方法和接口支持

NMocker 支持对各种类型的成员方法进行 Stub：

### 接口方法
```csharp
public interface ITarget
{
    int Method();
}

var target = new TargetImpl(); // 实现了 ITarget

// 使用 Lambda 表达式
Mocker.When(() => target.Method()).ThenReturn(5);

// 使用类型和方法名（适用于动态对象）
Mocker.When(target.GetType(), "Method").ThenReturn(5);
```

### 抽象方法
```csharp
public abstract class AbstractTarget
{
    public abstract int Method();
    protected abstract int ProtectedMethod();
}

var target = new ConcreteTarget(); // 继承自 AbstractTarget

// 公共抽象方法
Mocker.When(typeof(ConcreteTarget), "Method").ThenReturn(5);

// 受保护的抽象方法
Mocker.When(typeof(ConcreteTarget), "ProtectedMethod").ThenReturn(5);
```

### 虚拟方法
```csharp
public class BaseTarget
{
    public virtual int Method() => 12306;
    protected virtual int ProtectedMethod() => 12306;
}

var target = new DerivedTarget(); // 继承自 BaseTarget

// 重写的虚拟方法
Mocker.When(typeof(DerivedTarget), "Method").ThenReturn(5);

// 受保护的虚拟方法
Mocker.When(typeof(DerivedTarget), "ProtectedMethod").ThenReturn(5);
```

## 方法重载处理

当存在方法重载时，NMocker 会根据参数类型自动选择正确的重载：

### 静态方法重载
```csharp
public static int Method(int i) { return 100; }
public static int Method(string s) { return 200; }

Mocker.When(() => Target.Method(1)).ThenReturn(5);       // int 重载
Mocker.When(() => Target.Method("hello")).ThenReturn(20); // string 重载
```

### 实例方法重载
```csharp
public int Method(int i) { return 100; }
public int Method(string s) { return 200; }

var target = new Target();
Mocker.When(() => target.Method(1)).ThenReturn(5);       // int 重载
Mocker.When(() => target.Method("hello")).ThenReturn(20); // string 重载
```

对于可能存在歧义的情况，使用 `typeof()` 和方法名的方式：

```csharp
// 当参数为 null 时可能产生歧义
Mocker.When(typeof(Target), "Method", null).ThenReturn(1);
```

## 覆盖行为

后注册的 Stub 会覆盖先注册的：

### 静态方法覆盖
```csharp
Mocker.When(() => Target.Method()).ThenReturn(5);
Mocker.When(() => Target.Method()).ThenReturn(10); // 覆盖前面的设置

Assert.AreEqual(10, Target.Method()); // 返回 10
```

### 实例方法覆盖
```csharp
var target = new Target();
Mocker.When(() => target.Method()).ThenReturn(5);
Mocker.When(() => target.Method()).ThenReturn(10); // 覆盖前面的设置

Assert.AreEqual(10, target.Method()); // 返回 10
```

## 特殊情况处理

### 接口和继承
```csharp
// 支持接口类型匹配
Mocker.When(() => Target.Method(Arg.Any<IObject>())).ThenReturn(100);

// 支持子类匹配
Mocker.When(() => Target.Method(Arg.Any<DefaultObject>())).ThenReturn(100);

// 特定实例匹配
object obj = new object();
Mocker.When(() => Target.Method(Arg.Is(obj))).ThenReturn(100);
```

## 错误处理

### 方法不存在
```csharp
// 抛出 ArgumentException: "No matching method found"
```

### 方法重载歧义
```csharp
// 抛出 ArgumentException 列出所有候选方法
```

### 意外调用
```csharp
// 抛出 UnexpectedCallException 包含详细调用栈信息
```

## 最佳实践

1. **始终在测试初始化时调用 `Mocker.Clear()`**
2. **使用具体的参数匹配而非过度使用 `Arg.Any<T>()`**
3. **对于复杂的验证场景，使用 Mock 模式而非 Stub 模式**
4. **私有方法测试时使用 `typeof()` 和方法名的方式**
5. **合理使用验证链来验证多个方法调用**
6. **实例方法推荐使用 Lambda 表达式语法**
7. **对于动态对象或复杂继承场景，使用类型和方法名的方式**

## 完整示例

```csharp
[TestClass]
public class ExampleTest
{
    [TestInitialize]
    public void Setup()
    {
        Mocker.Clear();
    }

    [TestMethod]
    public void StaticMethodExample()
    {
        // Stub 静态方法
        Mocker.When(() => Calculator.Add(Arg.Any<int>(), Arg.Any<int>()))
              .Then(args => ((int)args[0]) + ((int)args[1]));
        
        Mocker.When(() => Calculator.IsEven(Arg.That<int>(i => i % 2 == 0)))
              .ThenReturn(true);
        
        // 验证行为
        int result = Calculator.Add(2, 3);
        Assert.AreEqual(5, result);
        
        bool isEven = Calculator.IsEven(4);
        Assert.IsTrue(isEven);
    }

    [TestMethod]
    public void InstanceMethodExample()
    {
        var calculator = new Calculator();
        
        // Stub 实例方法
        Mocker.When(() => calculator.Multiply(Arg.Any<int>(), Arg.Any<int>()))
              .Then(args => ((int)args[0]) * ((int)args[1]));
        
        // Stub 实例属性
        Mocker.When(() => calculator.LastResult).ThenReturn(100);
        
        // 验证行为
        int result = calculator.Multiply(3, 4);
        Assert.AreEqual(12, result);
        
        Assert.AreEqual(100, calculator.LastResult);
    }

    [TestMethod]
    public void MockAndVerifyExample()
    {
        var logger = new Logger();
        
        // Mock 模式验证
        Mocker.Mock(() => logger.Log(Arg.Any<string>()));
        
        // ... 执行被测代码 ...
        logger.Log("Expected message");
        
        Verifier.Once().Called(() => logger.Log("Expected message")).Verify();
    }

    [TestMethod]
    public void InterfaceExample()
    {
        IService service = new ServiceImpl();
        
        // 接口方法 Stub
        Mocker.When(() => service.Process(Arg.Any<string>())).ThenReturn("Stubbed");
        
        string result = service.Process("input");
        Assert.AreEqual("Stubbed", result);
    }
}
```

