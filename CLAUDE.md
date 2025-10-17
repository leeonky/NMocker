# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NMocker is a C# mocking/stubbing framework that uses the Harmony library for runtime method patching. It enables precise control over static and instance method behavior for unit testing without requiring dependency injection or design changes.

## Build and Test Commands

### Building the Project
```bash
# Build the entire solution
dotnet build NMocker.sln

# Build in Release mode
dotnet build NMocker.sln -c Release

# Build only the main library
dotnet build NMocker/NMocker.csproj

# Build only the test project
dotnet build TestNMocker/TestNMocker.csproj
```

### Running Tests
```bash
# Run all tests
dotnet test TestNMocker/TestNMocker.csproj

# Run tests with verbose output
dotnet test TestNMocker/TestNMocker.csproj -v normal

# Run a specific test class
dotnet test TestNMocker/TestNMocker.csproj --filter "FullyQualifiedName~TestVerifyTimesAndHit"

# Run a specific test method
dotnet test TestNMocker/TestNMocker.csproj --filter "TestMethod=no_call_and_any_expectation"
```

### Package Management
```bash
# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

## Architecture Overview

### Core Components

**Mocker (NMocker/Mocker.cs)**
- Main entry point for setting up mocks and stubs
- Contains static methods for `When()`, `Mock()`, and `WhenVoid()` operations
- Uses Harmony library to patch methods at runtime via `Prefix()` method
- Manages a global list of active mockers

**InvocationMatcher (NMocker/InvocationMatcher.cs)**
- Matches method calls based on method signature and argument patterns
- Handles both Lambda expressions and Type/MethodName combinations
- Manages Harmony patches and method resolution
- Supports method overload disambiguation

**Verifier (NMocker/Verifier.cs)**
- Provides verification capabilities for mocked method calls
- Supports fluent API for chaining multiple verifications
- Tracks call counts with `Times()`, `AtLeast()`, `AtMost()`, `Once()` methods
- Generates detailed error messages with call stack information

**Arg (NMocker/Arg.cs)**
- Argument matching system with various matcher types
- Supports `Any<T>()`, `Is<T>()`, `That<T>()` for flexible parameter matching
- Handles ref/out parameters through `Ref()` and `Out()` methods
- Type-safe argument validation

**Then (NMocker/Then.cs)**
- Defines behavior when mocked methods are called
- Supports returning values, calling original methods, or custom lambda actions
- Handles both value-returning and void methods

### Key Design Patterns

1. **Harmony Integration**: Uses Harmony library for runtime IL patching to intercept method calls
2. **Fluent API**: Provides chainable method calls for readable test setup
3. **Expression Trees**: Leverages C# expression trees for type-safe method specification
4. **Strategy Pattern**: Different `Then` implementations for various stubbing behaviors

### Test Structure

Tests are organized by functionality:
- **MockStatic.cs**: Core verification and mocking tests with detailed message validation
- **StubPublicStaticMethod.cs**: Static method stubbing tests
- **StubMemberMethodOfVirtualVariants.cs**: Virtual/abstract/interface method tests
- **StubPublicMemberMethodOfNonVirtualClass.cs**: Instance method stubbing tests
- **StubPrivateMemberMethodOfNonVirtualClass.cs**: Private method testing

### Important Implementation Details

1. **Method Patching**: All intercepted methods use Harmony's prefix patching to redirect calls
2. **Argument Matching**: Complex type hierarchy for matching different argument patterns
3. **Call Tracking**: Global `Invocation.invocations` list tracks all intercepted calls for verification
4. **Cleanup**: Always call `Mocker.Clear()` in test setup to reset state between tests

### Framework Targets
- **NMocker Library**: .NET Framework 4.5
- **Test Project**: .NET Framework 4.7.2
- **Dependencies**: Harmony 2.3.3, MSTest framework

### Common Development Patterns

When adding new test cases:
1. Always call `Mocker.Clear()` in `[TestInitialize]` methods
2. Use `Mocker.When()` for stubbing, `Mocker.Mock()` for verification scenarios
3. For private methods, use `typeof(Class)` and method name strings
4. Verification tests should use the `TestBase` class for consistent error message testing

When extending functionality:
1. Method resolution happens in `InvocationMatcher.FindMethodOrGetter/Setter`
2. New argument matchers should inherit from `ArgMatcher`
3. New stubbing behaviors should implement the `Then` interface
4. All new patches must be registered through `InvocationMatcher.PatchMethod`

# Development Process

## Steps
Every code change goes through one of the following three phases. Proceed step by step but please stop when one step is complete. After running any tests, please show the test run results.

### Add New Tests
The main goal of this phase is to add or modify test code based on user requirements, ultimately achieving failing test information that is sufficient to determine that the code behavior **does not conform** to the test requirements. **Never modify production code in this step**. After modifying the tests, you can run the tests directly without confirmation.

When generating test scenarios, **be sure not to add unnecessary test data** and ensure proper indentation alignment.

### Add Functionality
The main goal of this phase is to modify implementation code to make it pass the newly added test code and pass all existing tests. **Please be sure to obtain test run results and confirm they pass**. After modifying implementation code, you can run tests directly and confirm they pass. **Please be sure to write only the minimal implementation code to pass the tests**.

### Refactoring
The main goal of this phase is to modify implementation code according to user prompts to adjust the design. Or perform automatic review to remove duplicate code, redundant design, and inconsistent design. This process should not break existing tests. After modifying any code, please run tests and confirm they pass. **After refactoring is complete, please run all tests, not just builds**.
