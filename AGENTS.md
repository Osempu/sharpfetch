# SharpFetch - Agent Development Guide

## Project Overview
SharpFetch is a cross-platform CLI system information tool similar to fastfetch, built with .NET 9 and C#. It displays system information through various output formats (panels, trees, minimal views) using a modular architecture.

## Build/Test Commands

### Build
```bash
dotnet build
dotnet build -c Release
```

### Run
```bash
dotnet run
dotnet run -- --no-icons
dotnet run -- --minimal
```

### Publish
```bash
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-x64
```

### Clean
```bash
dotnet clean
```

### Tests
**Note:** No test project exists yet. When adding tests, use:
```bash
dotnet test
dotnet test --filter "FullyQualifiedName~ModuleName"  # Single test class
dotnet test --filter "Name~SpecificTestMethod"        # Single test method
```

## Architecture & Design Principles

### Module-Based Architecture
Follow the **fastfetch modular pattern** for system information gathering:
- Each system info category should be a separate, self-contained module
- Modules should be independently queryable and configurable
- New modules should be easily added without modifying existing code
- Consider creating a `Modules/` directory structure as the project grows

### Scalability Guidelines
- **Separation of Concerns**: Keep data collection (sysinfo.cs), presentation (panels.cs, graphics.cs), and utilities (helpers.cs) separate
- **Cross-Platform**: Always use `RuntimeInformation.IsOSPlatform()` checks for platform-specific code
- **Performance**: Minimize P/Invoke calls; cache expensive operations when possible
- **Extensibility**: Design for easy addition of new output formats and system info modules

## Code Style Guidelines

### File Organization
- One primary class per file, matching filename (e.g., `Sysinfo` → `sysinfo.cs`)
- Use lowercase filenames for C# source files (current convention)
- Group related functionality: data models, helpers, UI/presentation logic

### Namespace & Imports
```csharp
// Standard imports first, then third-party
using System.Diagnostics;
using System.Runtime.InteropServices;
using Spectre.Console;

// Single namespace declaration per file
namespace sharpfetch;
```

### Naming Conventions
- **Classes**: PascalCase (`Sysinfo`, `MemoryInfo`, `SharpFetchGraphics`)
- **Methods**: PascalCase with descriptive names (`GetCpu()`, `GetWindowsMemoryInfo()`)
- **Properties**: PascalCase (`TotalPhysicalMemoryBytes`, `CPU`, `MemoryInfo`)
- **Private fields**: camelCase with underscore prefix (`_sysInfo`)
- **Parameters/locals**: camelCase (`hideIcons`, `isMinimal`)
- **Static utility classes**: Static class + PascalCase methods (`Helpers.ToGB()`, `Helpers.Execute()`)

### Type Annotations & Nullable
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- Use nullable annotations appropriately (`string?` for nullable strings)
- Use null-coalescing operators: `?? "Unknown"`
- Prefer pattern matching: `if (!string.IsNullOrWhiteSpace(value))`

### Formatting
- **Braces**: Opening brace on same line for methods and types (follow existing style)
- **Indentation**: 4 spaces (no tabs)
- **Properties**: Expression-bodied members for simple getters: `public string CPU => GetCpu();`
- **String interpolation**: Use `$"{value}"` over concatenation
- **Collection initialization**: Use collection expressions where appropriate

### Error Handling
- **Graceful degradation**: Return "Unknown" for system info failures
- **Try-catch**: Use for external process calls and P/Invoke
- **No exceptions for control flow**: Let errors bubble only when critical
- **Empty catch blocks**: Acceptable for non-critical system info gathering (current pattern)

```csharp
try
{
    return Helpers.Execute("powershell", "-NoProfile -Command ...");
}
catch
{
    return "Unknown";  // Graceful fallback
}
```

### P/Invoke & Interop
- Use `LibraryImport` for new code (C# 11+) when possible
- Mark P/Invoke structs as `internal` or `private`
- Use `StructLayout` attributes for unmanaged structs
- Handle unsafe blocks when needed (enabled: `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`)

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal class MEMORYSTATUSEX { ... }

[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
```

### Platform-Specific Code Pattern
```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows implementation
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Linux implementation
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    // macOS implementation
}
return "Unknown";  // Fallback
```

## Dependencies
- **Spectre.Console** (v0.54.0): Rich terminal UI
- **System.CommandLine** (v2.0.5): CLI argument parsing

## Development Workflows

### Adding a New System Info Module
1. Add property to `Sysinfo` class: `public string NewInfo => GetNewInfo();`
2. Implement platform-specific `GetNewInfo()` method
3. Update all output formats in `panels.cs` to include the new info
4. Test across Windows, Linux, and macOS if possible

### Adding a New Output Format
1. Add static method to `ConsoleOutput` class: `public static void PrintAsNewFormat(Sysinfo sysInfo)`
2. Use Spectre.Console widgets (Panel, Grid, Tree, BreakdownChart, etc.)
3. Update `SharpFetchGraphics.printSysInfo()` to call new format
4. Consider adding a CLI flag to toggle formats

### Best Practices
- **Keep it cross-platform**: Test on multiple operating systems
- **Fail gracefully**: Always provide fallbacks for missing system info
- **Use Spectre.Console**: Leverage rich formatting and color capabilities
- **Document platform limitations**: Note when features are OS-specific
- **Minimize allocations**: Use `Span<T>`, `ReadOnlySpan<T>` in hot paths when beneficial
- **Async when I/O-bound**: Consider async for file reads and process execution in future iterations

## .NET & C# Specific Guidelines
- Target: **.NET 9** (`net9.0`)
- Language: **C# 13** (implicit with .NET 9)
- Use modern C# features: pattern matching, records (when appropriate), file-scoped namespaces
- Prefer `var` for local variables when type is obvious
- Use expression-bodied members for simple properties and methods
- Leverage `ImplicitUsings` for common namespaces

## Future Considerations
- Add unit tests (xUnit recommended for .NET projects)
- Implement configuration file support (JSON/TOML)
- Add module selection flags (e.g., `--modules cpu,memory,gpu`)
- Consider Native AOT compilation for faster startup and smaller binaries
- Implement color scheme customization
- Add benchmark tests for performance-critical paths (use BenchmarkDotNet)
