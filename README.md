# Mocker 库使用文档

## 简介

Mocker 是一个为 C# 设计的 Stub 库，用于通过覆盖静态方法的行为来帮助创建灵活且可控的测试场景。这个库允许您为测试中的静态方法指定返回值和行为，通过隔离工作单元与其依赖关系，使单元测试更加集中和可靠。

## 基本用法

1. **设置**
   在每个测试之前始终清除任何现有的 Stub，以确保一个干净的测试环境：

   ```csharp
   [TestInitialize]
   public void Setup()
   {
       Mocker.Clear();
   }
   ```

2. **Stub 静态方法**
   Stub 一个静态方法，当调用时返回一个特定的值：

   ```csharp
   Mocker.When(() => Target.method()).ThenReturn(5);
   ```

3. **断言行为**
   Stub 之后，您方法返回预期的Stub的值：

   ```csharp
   Assert.AreEqual(5, Target.method());
   ```

## 高级用法

### 多个返回值

您可以根据输入参数为同一方法指定不同的返回值：

```csharp
Mocker.When(() => Target.method1(1)).ThenReturn(5);
Mocker.When(() => Target.method1(2)).ThenReturn(10);

Assert.AreEqual(5, Target.method1(1));
Assert.AreEqual(10, Target.method1(2));
```

### 参数匹配器

**通用匹配器：**
使用 `Arg.Any<T>()` 匹配指定类型的任何参数：

```csharp
Mocker.When(() => Target.method1(Arg.Any<int>())).ThenReturn(5);

Assert.AreEqual(5, Target.method1(100));  // 对于任何 int 参数都将返回 5
```

**自定义匹配器：**
使用 `Arg.That<T>(Predicate<T>)` 根据自定义条件匹配参数：

```csharp
Mocker.When(() => Target.method1(Arg.That<int>(i => i > 5))).ThenReturn(5);

Assert.AreEqual(0, Target.method1(5));  // 返回 0 因为 5 不大于 5
Assert.AreEqual(5, Target.method1(6));  // 返回 5 因为 6 大于 5
```

### 重置 Stub

清除所有 Stub 并将方法恢复到其原始行为：

```csharp
Mocker.Clear();

Assert.AreEqual(100, Target.method());  // 原始行为被恢复
```

