---
description: "Coding guidelines for C# code"
applyTo: "**/*.cs"
---

# C# Coding Guidelines

## General

- Make sure that source code line length does not exceed 110 characters.
- Method and property accessor should not exceed 40 lines.
- Files should not exceed 400 lines.
- Always use the latest version of C#.
- Check for warnings from code analysis tools in copilot generated code.
- Prefer explicit variable types.
- Use target-typed new expressions.
- Trust the C# null annotations in .NET base class library and don't add null checks when the type system says a value cannot be null.
- Do not add `returns` tag to XML documentation.
- Avoid using `#region` in new code.
- Use pattern matching and `is`/`or`/`and` patterns. Prefer `is` patterns and C# pattern matching over manual type checks and comparisons. Use named parameters for boolean arguments.
- Sealed classes do not need the full Dispose pattern. A simple `Dispose()` is sufficient since no derived class can introduce a finalizer.

## .NET API Usage

- **Logging:** Use globally available methods LogVerbose, LogInformation, LogWarning, LogError and LogCritical for logging.
    - **LogVerbose:** Use for detailed debugging information like preliminary calculation results and logical steps checkpoints.
    - **LogInformation:** Use for informational messages that highlight the progress of the application at coarse-grained level.
    - **LogWarning:** Use for potentially harmful situations or unexpected events that do not cause the application to stop.
    - **LogError:** Use for errors and exceptions that cannot be handled and may require attention, but do not cause the application to crash.
    - **LogCritical:** Use for critical errors that cause the application to terminate or require immediate attention.
- **Validation** Use `ArgumentNullException.ThrowIfNull` and similar APIs to validate input parameters in public methods, `Verify.*` methods for more complex validation.
- Use `Environment.ProcessPath` and `AppContext.BaseDirectory` instead of `Process.GetCurrentProcess().MainModule?.FileName` and `Assembly.Location` for NativeAOT/single-file compatibility.

## Error Handling and Assertions

- Use `Debug.Assert` for internal invariants, not exceptions.
- Use `ThrowIf` helpers over manual checks. Use `ArgumentOutOfRangeException.ThrowIfNegative`, `ObjectDisposedException.ThrowIf`, etc. instead of manual if-then-throw patterns.
- Include actionable details in exception messages. Use `nameof` for parameter names. Include the unsupported type or unexpected value. Never throw empty exceptions.
- Do not catch critical errors like `OutOfMemoryExceptions`. Use `ex.IsCritical()` extension method when generic exception type is used in the `catch` block.
- Avoid exception swallowing that masks unexpected errors. Do not use try/catch blocks that silently discard exceptions (`catch { continue; }`, `catch { return null; }`).
- Prefer using custom exception types based on `Common.Diagnostics.Exception<T>`.

## Unit Testing

- Use Arrange/Act/Assert pattern.
- Copy existing style in nearby files for test method names and capitalization. Unit test method names should follow snake case convention, but first letter of the method name should be in upper case.
- Prefer keeping tests near the unit under test and use the existing `...Tests` naming convention.
- Unit tests use TUnit. Reuse the local base class pattern already established in the target test project such as `TestBase`, `EngineTestBase`, or `DataTestBase` where appropriate.
- Assertions should be implemented using `Shouldly`.
- Test data generation can use `AutoFaker` via the helpers already exposed by the local test base classes when appropriate.
- For mocks and stubs use `NSubstitute`.

## Performance & Allocations

### Allocation Avoidance

- Pre-allocate collections when size is known. Pass capacity to `Dictionary`, `HashSet`, `List` constructors when the expected count is available.
- Structs in dictionaries need `IEquatable<T>` and `GetHashCode`. Without these, the runtime falls back to boxing allocations for equality comparison.
- Use `stackalloc` to allocate temporary buffers up to ~1KB and validate size. Don't stackalloc based on user-controlled or large input sizes. Move stackalloc to just before usage, not before early returns.

### Code Structure for Performance

- Place cheap checks before expensive operations. Order conditionals so cheapest/most-common checks come first. Move expensive work after early-exit checks.
- Allocate resources lazily where possible. Allocate expensive resources on first use, not during initialization. Avoid forcing type initialization during startup.

### Specific API Choices

- Use `FrozenDictionary` instead of `Dictionary` for long-living static data maps.
- Use `ArrayPool<T>` for large temporary buffers to reduce GC pressure.
- Use `Span<T>` and `Memory<T>` for high-performance memory access without allocations.
