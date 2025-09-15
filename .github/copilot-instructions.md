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

# Development Process

## Steps
Every code change goes through one of the following three phases. Proceed step by step. After running any tests, please show the test run results.

### Add New Tests
The main goal of this phase is to add or modify test code based on user requirements, ultimately achieving failing test information that is sufficient to determine that the code behavior **does not conform** to the test requirements. **Never modify production code in this step**. After modifying the tests, you can run the tests directly without confirmation.

When generating test scenarios, **be sure not to add unnecessary test data** and ensure proper indentation alignment.

### Add Functionality
The main goal of this phase is to modify implementation code to make it pass the newly added test code and pass all existing tests. **Please be sure to obtain test run results and confirm they pass**. After modifying implementation code, you can run tests directly and confirm they pass. **Please be sure to write only the minimal implementation code to pass the tests**.

### Refactoring
The main goal of this phase is to modify implementation code according to user prompts to adjust the design. Or perform automatic review to remove duplicate code, redundant design, and inconsistent design. This process should not break existing tests. After modifying any code, please run tests and confirm they pass. **After refactoring is complete, please run all tests, not just builds**.