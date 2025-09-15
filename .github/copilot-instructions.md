# NMocker AI Coding Instructions

## Project Overview
NMocker is a C# static method mocking/stubbing framework that uses Harmony for runtime method patching. It enables precise control over static method behavior in unit tests without requiring dependency injection or design changes.

## Architecture Components

### Core Classes & Responsibilities
- **`Mocker`** (`NMocker/Mocker.cs`): Central API entry point with `When()` fluent interface, manages global mocker registry
- **`InvocationMatcher`** (`NMocker/InvocationMatcher.cs`): Matches method calls by signature and arguments using expression trees
- **`Invocation`** (`NMocker/Invocation.cs`): Captures actual method call details with stack trace for verification
- **`Verifier`** (`NMocker/Verifier.cs`): Provides call verification DSL (`Times()`, `Once()`, `AtLeast()`)
- **`Then`** hierarchy (`NMocker/Then.cs`): Strategy pattern for stub behaviors (`ThenValue`, `ThenLambda`, `ThenActual`)

### Key Design Patterns

**Harmony Integration**: Uses `HarmonyLib` prefix patching via `Prefix()` method to intercept static calls
```csharp
protected static bool Prefix(MethodBase __originalMethod, object[] __args, ref object __result)
```

**Expression Tree DSL**: Fluent API uses lambda expressions for type-safe method matching:
```csharp
Mocker.When(() => Target.method()).ThenReturn(5);
```

**Last-Win Semantics**: Multiple stubs for same method - last registered wins (see `mockers.LastOrDefault()` in `Mocker.Prefix()`)

## Project-Specific Conventions

### Test Structure Patterns
- Always call `Mocker.Clear()` in `[TestInitialize]` to reset Harmony patches
- Use static inner `Target` classes in test files to isolate test subjects
- Track original method calls with `static bool called` flags for verification
- Follow naming: `StubPublicStaticMethod.cs`, `StubPrivetStaticProperty.cs` etc.

### Argument Matching System
- `Arg.Any<T>()`: Type-based wildcard matching
- `Arg.Is(value)`: Exact value matching  
- `Arg.That<T>(predicate)`: Custom predicate matching
- Ref/out parameter support via `ProcessRefAndOutArgs()`

### Error Handling Patterns
- `UnexpectedCallException` with detailed call stack traces
- Ambiguous method resolution throws with all candidates listed
- Stack frame tracking for precise error location reporting

## Build & Test Workflow

### Solution Structure
- **NMocker.sln**: Contains `NMocker` (library) + `TestNMocker` (MSTest project)
- **Target Framework**: .NET Framework 4.5+ (library), 4.7.2 (tests)
- **Dependencies**: Harmony 2.3.3, MSTest 2.1.2, NUnit 4.2.2

### Test Execution
- Use MSTest runner (not NUnit despite package presence)
- Tests in `TestNMocker/` verify both positive and negative scenarios
- `Bugs.cs` contains regression tests for specific edge cases

### Development Commands
```bash
# Build solution
dotnet build NMocker.sln

# Run tests  
dotnet test TestNMocker/TestNMocker.csproj
```

## Critical Implementation Details

### Method Resolution Logic
- Supports both methods and properties (getters/setters)
- Handles method overloading via parameter type matching
- Property access translated to get/set method calls

### Memory Management
- Global static collections in `Mocker.mockers` and `InvocationMatcher`
- Must call `Clear()` to prevent test pollution
- Harmony patches persist until explicitly cleared

### Verification System
- Captures all invocations for post-test verification
- Supports fluent verification: `Verifier.Called(expression).Times(n)`
- Line number tracking for detailed failure messages

## When Adding Features
- Follow existing fluent API patterns (`When().Then*()`)
- Add corresponding test cases in `TestNMocker/`
- Consider Harmony limitations and .NET Framework compatibility
- Maintain expression tree parsing in `InvocationMatcher.Create()`