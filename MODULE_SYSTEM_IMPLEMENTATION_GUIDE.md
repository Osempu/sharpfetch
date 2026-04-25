# SharpFetch Module System & Configuration Implementation Guide

## Table of Contents
1. [Overview](#overview)
2. [Architecture Recommendations](#architecture-recommendations)
3. [Implementation Plan](#implementation-plan)
4. [Code Examples](#code-examples)
5. [Configuration System](#configuration-system)
6. [Performance Considerations](#performance-considerations)
7. [Testing Strategy](#testing-strategy)

---

## Overview

This guide provides a complete implementation plan for a **modular, performant, and flexible** system information framework for SharpFetch, inspired by fastfetch's architecture.

### Design Goals
- ✅ **Modularity**: Each system info category is independent and self-contained
- ✅ **Performance**: Lazy evaluation, parallel execution, caching where appropriate
- ✅ **Flexibility**: Users can select which modules to run via CLI or config
- ✅ **Extensibility**: Easy to add new modules without modifying core code
- ✅ **Configuration**: JSON-based configuration with CLI overrides

---

## Architecture Recommendations

### 1. **Interface-Based Module System**

**Recommendation**: Use an interface-based plugin architecture where each system info category is a separate module.

**Why**:
- Loose coupling between modules and core application
- Easy to add/remove/disable modules
- Supports lazy loading and parallel execution
- Simplifies unit testing

### 2. **Module Registry Pattern**

**Recommendation**: Implement a central module registry that discovers and manages all available modules.

**Why**:
- Single source of truth for available modules
- Supports dynamic module discovery
- Easy to query which modules are available
- Simplifies dependency injection

### 3. **Lazy Evaluation with Caching**

**Recommendation**: Use `Lazy<T>` for expensive operations with optional caching.

**Why**:
- Only compute values when actually needed
- Prevents duplicate expensive operations
- Supports "on-demand" module execution
- Reduces startup time

### 4. **Async/Parallel Execution**

**Recommendation**: Support async module execution with `Task.WhenAll()` for parallel processing.

**Why**:
- Multiple independent modules can execute concurrently
- Significantly faster for I/O-bound operations (file reads, process execution)
- Better resource utilization

### 5. **Options Pattern for Configuration**

**Recommendation**: Use `Microsoft.Extensions.Configuration` and Options pattern for configuration management.

**Why**:
- Industry-standard .NET approach
- Supports multiple config sources (JSON, env vars, CLI args)
- Strong typing and validation
- Hot-reload capability (if needed in future)

---

## Implementation Plan

### Phase 1: Core Module Infrastructure (Priority: High)

1. **Create Module Abstractions**
   - Define `IModule` interface
   - Create `ModuleMetadata` class
   - Implement `ModuleResult` record

2. **Implement Module Registry**
   - Create `ModuleRegistry` class
   - Add auto-discovery mechanism
   - Support module filtering

3. **Create Base Modules**
   - Convert existing system info to modules (CPU, Memory, GPU, etc.)
   - Each module implements `IModule`

### Phase 2: Configuration System (Priority: High)

1. **Add Configuration Infrastructure**
   - Install `Microsoft.Extensions.Configuration.Json` NuGet package
   - Create configuration model classes
   - Implement configuration loader

2. **Create Default Configuration**
   - Define `config.json` schema
   - Implement defaults and validation
   - Support user config overrides

3. **Integrate with CLI**
   - CLI args override config values
   - Support `--modules` flag for runtime selection
   - Support `--config` flag for custom config path

### Phase 3: Execution Engine (Priority: Medium)

1. **Implement Module Executor**
   - Sequential execution strategy
   - Parallel execution strategy
   - Error handling and fallbacks

2. **Add Output Formatting**
   - Refactor existing output formats to consume module results
   - Support dynamic module rendering

### Phase 4: Advanced Features (Priority: Low)

1. **Module Dependencies**
2. **Module Caching**
3. **Module Profiling**
4. **Custom Module Loading**

---

## Code Examples

### 1. Module Interface & Metadata

```csharp
// File: Modules/IModule.cs
namespace sharpfetch.Modules;

/// <summary>
/// Defines a system information module that can be executed independently.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the unique identifier for this module (e.g., "cpu", "memory", "gpu").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name for this module (e.g., "CPU", "Memory", "GPU").
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of what this module provides.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the priority/order for display (lower numbers displayed first).
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Gets whether this module is enabled by default.
    /// </summary>
    bool EnabledByDefault { get; }

    /// <summary>
    /// Executes the module and returns the result.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The module execution result.</returns>
    Task<ModuleResult> ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a module execution.
/// </summary>
public record ModuleResult
{
    public required string ModuleId { get; init; }
    public required string DisplayName { get; init; }
    public bool Success { get; init; }
    public string? Value { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ExecutionTime { get; init; }

    public static ModuleResult CreateSuccess(string moduleId, string displayName, string value, TimeSpan executionTime)
        => new()
        {
            ModuleId = moduleId,
            DisplayName = displayName,
            Success = true,
            Value = value,
            ExecutionTime = executionTime
        };

    public static ModuleResult CreateError(string moduleId, string displayName, string errorMessage, TimeSpan executionTime)
        => new()
        {
            ModuleId = moduleId,
            DisplayName = displayName,
            Success = false,
            ErrorMessage = errorMessage,
            ExecutionTime = executionTime
        };
}
```

### 2. Abstract Base Module

```csharp
// File: Modules/ModuleBase.cs
namespace sharpfetch.Modules;

/// <summary>
/// Base class for all modules providing common functionality.
/// </summary>
public abstract class ModuleBase : IModule
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public virtual string Description => string.Empty;
    public virtual int Order => 100;
    public virtual bool EnabledByDefault => true;

    public async Task<ModuleResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var value = await GetValueAsync(cancellationToken);
            stopwatch.Stop();
            
            return ModuleResult.CreateSuccess(Id, DisplayName, value, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return ModuleResult.CreateError(Id, DisplayName, ex.Message, stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Derived classes implement this to provide the actual system information.
    /// </summary>
    protected abstract Task<string> GetValueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Helper for synchronous operations.
    /// </summary>
    protected Task<string> FromResult(string value) => Task.FromResult(value);

    /// <summary>
    /// Helper for wrapping synchronous operations.
    /// </summary>
    protected async Task<string> RunAsync(Func<string> operation, CancellationToken cancellationToken)
    {
        return await Task.Run(operation, cancellationToken);
    }
}
```

### 3. Example Module Implementations

```csharp
// File: Modules/CpuModule.cs
namespace sharpfetch.Modules;

public class CpuModule : ModuleBase
{
    public override string Id => "cpu";
    public override string DisplayName => "CPU";
    public override string Description => "Processor information";
    public override int Order => 10;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        // Wrap existing CPU detection logic
        return RunAsync(GetCpuInfo, cancellationToken);
    }

    private string GetCpuInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER", EnvironmentVariableTarget.Machine) 
                ?? "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var line = File.ReadLines("/proc/cpuinfo")
                .FirstOrDefault(l => l.StartsWith("model name"));
            return line?.Split(':').Last().Trim() ?? "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Helpers.Execute("sysctl", "-n machdep.cpu.brand_string");
        }
        
        return "Unknown";
    }
}

// File: Modules/MemoryModule.cs
namespace sharpfetch.Modules;

public class MemoryModule : ModuleBase
{
    public override string Id => "memory";
    public override string DisplayName => "Memory";
    public override string Description => "RAM usage information";
    public override int Order => 20;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(GetMemoryInfo, cancellationToken);
    }

    private string GetMemoryInfo()
    {
        // Reuse existing memory detection logic
        var sysinfo = new Sysinfo();
        return sysinfo.MemoryInfo.ToString();
    }
}

// File: Modules/GpuModule.cs
namespace sharpfetch.Modules;

public class GpuModule : ModuleBase
{
    public override string Id => "gpu";
    public override string DisplayName => "GPU";
    public override string Description => "Graphics card information";
    public override int Order => 30;
    public override bool EnabledByDefault => true;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(GetGpuInfo, cancellationToken);
    }

    private string GetGpuInfo()
    {
        // Reuse existing GPU detection logic
        var sysinfo = new Sysinfo();
        return sysinfo.GPU;
    }
}

// File: Modules/OsModule.cs
namespace sharpfetch.Modules;

public class OsModule : ModuleBase
{
    public override string Id => "os";
    public override string DisplayName => "OS";
    public override string Description => "Operating system information";
    public override int Order => 5;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return FromResult(Sysinfo.GetOSName());
    }
}
```

### 4. Module Registry

```csharp
// File: Modules/ModuleRegistry.cs
namespace sharpfetch.Modules;

/// <summary>
/// Central registry for all available modules.
/// </summary>
public class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new();
    private static readonly Lazy<ModuleRegistry> _instance = new(() => new ModuleRegistry());

    public static ModuleRegistry Instance => _instance.Value;

    private ModuleRegistry()
    {
        // Auto-discover and register all modules
        DiscoverModules();
    }

    /// <summary>
    /// Discovers all IModule implementations via reflection.
    /// </summary>
    private void DiscoverModules()
    {
        var moduleType = typeof(IModule);
        var modules = typeof(ModuleRegistry).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && moduleType.IsAssignableFrom(t))
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .ToList();

        foreach (var module in modules)
        {
            Register(module);
        }
    }

    /// <summary>
    /// Manually register a module.
    /// </summary>
    public void Register(IModule module)
    {
        _modules[module.Id] = module;
    }

    /// <summary>
    /// Get a specific module by ID.
    /// </summary>
    public IModule? GetModule(string id)
    {
        return _modules.TryGetValue(id, out var module) ? module : null;
    }

    /// <summary>
    /// Get all registered modules.
    /// </summary>
    public IEnumerable<IModule> GetAllModules()
    {
        return _modules.Values.OrderBy(m => m.Order);
    }

    /// <summary>
    /// Get modules filtered by IDs.
    /// </summary>
    public IEnumerable<IModule> GetModules(IEnumerable<string> ids)
    {
        return ids.Select(id => GetModule(id))
            .Where(m => m != null)
            .Cast<IModule>()
            .OrderBy(m => m.Order);
    }

    /// <summary>
    /// Get all enabled modules based on configuration.
    /// </summary>
    public IEnumerable<IModule> GetEnabledModules(ModuleConfiguration config)
    {
        if (config.Modules.Any())
        {
            // If specific modules are listed, use only those
            return GetModules(config.Modules);
        }

        // Otherwise, return all modules enabled by default
        return GetAllModules().Where(m => m.EnabledByDefault);
    }

    /// <summary>
    /// Get available module IDs.
    /// </summary>
    public IEnumerable<string> GetModuleIds()
    {
        return _modules.Keys.OrderBy(k => k);
    }
}
```

### 5. Module Executor

```csharp
// File: Modules/ModuleExecutor.cs
namespace sharpfetch.Modules;

/// <summary>
/// Executes modules and collects results.
/// </summary>
public class ModuleExecutor
{
    private readonly ModuleRegistry _registry;

    public ModuleExecutor(ModuleRegistry? registry = null)
    {
        _registry = registry ?? ModuleRegistry.Instance;
    }

    /// <summary>
    /// Execute modules sequentially.
    /// </summary>
    public async Task<IReadOnlyList<ModuleResult>> ExecuteSequentialAsync(
        IEnumerable<IModule> modules,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ModuleResult>();

        foreach (var module in modules)
        {
            var result = await module.ExecuteAsync(cancellationToken);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Execute modules in parallel for better performance.
    /// </summary>
    public async Task<IReadOnlyList<ModuleResult>> ExecuteParallelAsync(
        IEnumerable<IModule> modules,
        CancellationToken cancellationToken = default)
    {
        var tasks = modules.Select(m => m.ExecuteAsync(cancellationToken));
        var results = await Task.WhenAll(tasks);
        
        return results.OrderBy(r => modules.First(m => m.Id == r.ModuleId).Order).ToList();
    }

    /// <summary>
    /// Execute modules based on configuration.
    /// </summary>
    public async Task<IReadOnlyList<ModuleResult>> ExecuteFromConfigAsync(
        ModuleConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var modules = _registry.GetEnabledModules(config);

        return config.ParallelExecution
            ? await ExecuteParallelAsync(modules, cancellationToken)
            : await ExecuteSequentialAsync(modules, cancellationToken);
    }
}
```

---

## Configuration System

### 1. Configuration Models

```csharp
// File: Configuration/SharpFetchConfiguration.cs
namespace sharpfetch.Configuration;

/// <summary>
/// Root configuration for SharpFetch.
/// </summary>
public class SharpFetchConfiguration
{
    /// <summary>
    /// Module configuration.
    /// </summary>
    public ModuleConfiguration Modules { get; set; } = new();

    /// <summary>
    /// Display/output configuration.
    /// </summary>
    public DisplayConfiguration Display { get; set; } = new();
}

/// <summary>
/// Configuration for module execution.
/// </summary>
public class ModuleConfiguration
{
    /// <summary>
    /// List of module IDs to execute. Empty = all default modules.
    /// </summary>
    public List<string> Modules { get; set; } = new();

    /// <summary>
    /// Whether to execute modules in parallel.
    /// </summary>
    public bool ParallelExecution { get; set; } = true;

    /// <summary>
    /// Timeout per module in milliseconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Whether to show execution time for each module (debug).
    /// </summary>
    public bool ShowExecutionTime { get; set; } = false;
}

/// <summary>
/// Configuration for display/output formatting.
/// </summary>
public class DisplayConfiguration
{
    /// <summary>
    /// Output format: "panels", "trees", "minimal", "leftpanel"
    /// </summary>
    public string Format { get; set; } = "panels";

    /// <summary>
    /// Whether to show icons.
    /// </summary>
    public bool ShowIcons { get; set; } = true;

    /// <summary>
    /// Color scheme: "default", "rainbow", "monochrome"
    /// </summary>
    public string ColorScheme { get; set; } = "default";

    /// <summary>
    /// Whether to show charts for memory and disk.
    /// </summary>
    public bool ShowCharts { get; set; } = true;
}
```

### 2. Configuration Loader

```csharp
// File: Configuration/ConfigurationLoader.cs
namespace sharpfetch.Configuration;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Loads configuration from multiple sources with priority.
/// Priority: CLI args > User config file > Default config > Hardcoded defaults
/// </summary>
public class ConfigurationLoader
{
    private const string DefaultConfigFileName = "config.json";
    private const string UserConfigFileName = ".sharpfetch.json";

    /// <summary>
    /// Load configuration from all sources.
    /// </summary>
    public static SharpFetchConfiguration Load(string[]? args = null, string? customConfigPath = null)
    {
        var builder = new ConfigurationBuilder();

        // 1. Add default embedded configuration (if you have one)
        // builder.AddJsonFile("default-config.json", optional: true);

        // 2. Add user configuration from home directory
        var userConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            UserConfigFileName);
        
        if (File.Exists(userConfigPath))
        {
            builder.AddJsonFile(userConfigPath, optional: true);
        }

        // 3. Add custom configuration file if specified
        if (!string.IsNullOrEmpty(customConfigPath) && File.Exists(customConfigPath))
        {
            builder.AddJsonFile(customConfigPath, optional: false);
        }

        // 4. Add command-line arguments (highest priority)
        if (args != null && args.Length > 0)
        {
            builder.AddCommandLine(args);
        }

        var configuration = builder.Build();

        // Bind to strongly-typed configuration
        var config = new SharpFetchConfiguration();
        configuration.Bind(config);

        return config;
    }

    /// <summary>
    /// Create a default configuration file.
    /// </summary>
    public static void CreateDefaultConfigFile(string path)
    {
        var defaultConfig = new SharpFetchConfiguration
        {
            Modules = new ModuleConfiguration
            {
                Modules = new List<string>(), // Empty = all default modules
                ParallelExecution = true,
                TimeoutMs = 5000,
                ShowExecutionTime = false
            },
            Display = new DisplayConfiguration
            {
                Format = "panels",
                ShowIcons = true,
                ColorScheme = "default",
                ShowCharts = true
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(defaultConfig, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }
}
```

### 3. Default config.json Schema

```json
{
  "modules": {
    "modules": [
      // Empty array = all default modules
      // Or specify: ["os", "cpu", "memory", "gpu", "kernel"]
    ],
    "parallelExecution": true,
    "timeoutMs": 5000,
    "showExecutionTime": false
  },
  "display": {
    "format": "panels",
    "showIcons": true,
    "colorScheme": "default",
    "showCharts": true
  }
}
```

### 4. Example User Configurations

**Example 1: Minimal config (only CPU and Memory)**
```json
{
  "modules": {
    "modules": ["cpu", "memory"],
    "parallelExecution": true
  },
  "display": {
    "format": "minimal",
    "showIcons": false
  }
}
```

**Example 2: Full system info**
```json
{
  "modules": {
    "modules": ["os", "kernel", "cpu", "memory", "gpu", "disk", "uptime", "shell", "terminal"],
    "parallelExecution": true,
    "showExecutionTime": true
  },
  "display": {
    "format": "panels",
    "showIcons": true,
    "colorScheme": "rainbow",
    "showCharts": true
  }
}
```

---

## Updated Program.cs Integration

```csharp
// File: Program.cs
using System.CommandLine;
using sharpfetch;
using sharpfetch.Configuration;
using sharpfetch.Modules;

var rootCommand = new RootCommand("SharpFetch - Fast system information tool");

// Options
var configOption = new Option<string?>(
    aliases: new[] { "--config", "-c" },
    description: "Path to custom configuration file");

var modulesOption = new Option<string[]?>(
    aliases: new[] { "--modules", "-m" },
    description: "Comma-separated list of modules to display (e.g., cpu,memory,gpu)")
{
    AllowMultipleArgumentsPerToken = true
};

var formatOption = new Option<string?>(
    aliases: new[] { "--format", "-f" },
    description: "Output format: panels, trees, minimal, leftpanel");

var noIconsOption = new Option<bool>(
    "--no-icons",
    description: "Disable icons in output");

var parallelOption = new Option<bool>(
    "--parallel",
    getDefaultValue: () => true,
    description: "Execute modules in parallel");

var listModulesOption = new Option<bool>(
    "--list-modules",
    description: "List all available modules and exit");

var generateConfigOption = new Option<string?>(
    "--generate-config",
    description: "Generate a default configuration file at the specified path");

// Add options to root command
rootCommand.AddOption(configOption);
rootCommand.AddOption(modulesOption);
rootCommand.AddOption(formatOption);
rootCommand.AddOption(noIconsOption);
rootCommand.AddOption(parallelOption);
rootCommand.AddOption(listModulesOption);
rootCommand.AddOption(generateConfigOption);

rootCommand.SetHandler(async (context) =>
{
    var configPath = context.ParseResult.GetValueForOption(configOption);
    var modules = context.ParseResult.GetValueForOption(modulesOption);
    var format = context.ParseResult.GetValueForOption(formatOption);
    var noIcons = context.ParseResult.GetValueForOption(noIconsOption);
    var parallel = context.ParseResult.GetValueForOption(parallelOption);
    var listModules = context.ParseResult.GetValueForOption(listModulesOption);
    var generateConfig = context.ParseResult.GetValueForOption(generateConfigOption);

    // Handle --generate-config
    if (!string.IsNullOrEmpty(generateConfig))
    {
        ConfigurationLoader.CreateDefaultConfigFile(generateConfig);
        Console.WriteLine($"✓ Default configuration file created at: {generateConfig}");
        context.ExitCode = 0;
        return;
    }

    // Handle --list-modules
    if (listModules)
    {
        var registry = ModuleRegistry.Instance;
        Console.WriteLine("Available modules:");
        foreach (var module in registry.GetAllModules())
        {
            var status = module.EnabledByDefault ? "[enabled]" : "[disabled]";
            Console.WriteLine($"  {module.Id,-15} {status,-12} - {module.Description}");
        }
        context.ExitCode = 0;
        return;
    }

    // Load configuration
    var config = ConfigurationLoader.Load(context.ParseResult.Tokens.Select(t => t.Value).ToArray(), configPath);

    // Apply CLI overrides
    if (modules != null && modules.Length > 0)
    {
        config.Modules.Modules = modules.ToList();
    }

    if (format != null)
    {
        config.Display.Format = format;
    }

    if (noIcons)
    {
        config.Display.ShowIcons = false;
    }

    config.Modules.ParallelExecution = parallel;

    // Execute modules
    var executor = new ModuleExecutor();
    var results = await executor.ExecuteFromConfigAsync(config.Modules);

    // Display results based on format
    var renderer = new ModuleResultRenderer(config.Display);
    renderer.Render(results);

    context.ExitCode = 0;
});

return await rootCommand.InvokeAsync(args);
```

---

## Module Result Renderer

```csharp
// File: Rendering/ModuleResultRenderer.cs
namespace sharpfetch.Rendering;

using Spectre.Console;
using sharpfetch.Configuration;
using sharpfetch.Modules;

/// <summary>
/// Renders module results based on display configuration.
/// </summary>
public class ModuleResultRenderer
{
    private readonly DisplayConfiguration _config;

    public ModuleResultRenderer(DisplayConfiguration config)
    {
        _config = config;
    }

    public void Render(IReadOnlyList<ModuleResult> results)
    {
        switch (_config.Format.ToLowerInvariant())
        {
            case "panels":
                RenderAsPanels(results);
                break;
            case "trees":
                RenderAsTrees(results);
                break;
            case "minimal":
                RenderAsMinimal(results);
                break;
            case "leftpanel":
                RenderAsLeftPanel(results);
                break;
            default:
                RenderAsMinimal(results);
                break;
        }
    }

    private void RenderAsPanels(IReadOnlyList<ModuleResult> results)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn();

        foreach (var result in results.Where(r => r.Success))
        {
            var icon = GetIconForModule(result.ModuleId);
            var displayName = _config.ShowIcons ? $"{icon} {result.DisplayName}" : result.DisplayName;
            grid.AddRow($"[yellow]{displayName}[/]", $"[cyan]{result.Value}[/]");
        }

        AnsiConsole.Write(
            new Panel(grid)
                .Header("System Information", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Green));
    }

    private void RenderAsTrees(IReadOnlyList<ModuleResult> results)
    {
        var tree = new Tree("[green bold]System Information[/]");

        foreach (var result in results.Where(r => r.Success))
        {
            var icon = GetIconForModule(result.ModuleId);
            var displayName = _config.ShowIcons ? $"{icon} {result.DisplayName}" : result.DisplayName;
            tree.AddNode($"[yellow]{displayName}:[/] [cyan]{result.Value}[/]");
        }

        AnsiConsole.Write(tree);
    }

    private void RenderAsMinimal(IReadOnlyList<ModuleResult> results)
    {
        foreach (var result in results.Where(r => r.Success))
        {
            var icon = GetIconForModule(result.ModuleId);
            var displayName = _config.ShowIcons ? $"{icon} {result.DisplayName}" : result.DisplayName;
            Console.WriteLine($"{displayName}: {result.Value}");
        }
    }

    private void RenderAsLeftPanel(IReadOnlyList<ModuleResult> results)
    {
        // Similar to existing PrintAsLeftPanel implementation
        // Adapt to use ModuleResult
    }

    private string GetIconForModule(string moduleId)
    {
        return moduleId switch
        {
            "cpu" => "🖥️",
            "memory" => "💾",
            "gpu" => "🎮",
            "os" => "🖥️",
            "kernel" => "⚙️",
            "disk" => "💿",
            "uptime" => "⏱️",
            "shell" => "🐚",
            "terminal" => "📟",
            _ => "•"
        };
    }
}
```

---

## Performance Considerations

### 1. **Lazy Evaluation**
```csharp
// Only execute modules when actually needed
private readonly Lazy<string> _cpuInfo = new(() => GetCpuExpensive());

public string CPU => _cpuInfo.Value;
```

### 2. **Parallel Execution**
- Use `Task.WhenAll()` for independent modules
- Expected speedup: 2-3x for I/O-bound operations
- CPU-bound operations may see less benefit

### 3. **Caching Strategy**
```csharp
// For values that don't change during execution
private static readonly ConcurrentDictionary<string, string> _cache = new();

public string GetCachedValue(string key, Func<string> factory)
{
    return _cache.GetOrAdd(key, _ => factory());
}
```

### 4. **Timeout Handling**
```csharp
// Prevent slow modules from blocking entire execution
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(config.TimeoutMs));
var result = await module.ExecuteAsync(cts.Token);
```

---

## Testing Strategy

### 1. **Unit Tests for Modules**
```csharp
[Fact]
public async Task CpuModule_ShouldReturnValidResult()
{
    // Arrange
    var module = new CpuModule();

    // Act
    var result = await module.ExecuteAsync();

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Value);
    Assert.NotEqual("Unknown", result.Value);
}
```

### 2. **Integration Tests**
```csharp
[Fact]
public async Task ModuleExecutor_ShouldExecuteAllModules()
{
    // Arrange
    var config = new ModuleConfiguration { ParallelExecution = true };
    var executor = new ModuleExecutor();

    // Act
    var results = await executor.ExecuteFromConfigAsync(config);

    // Assert
    Assert.NotEmpty(results);
    Assert.All(results, r => Assert.True(r.Success || r.ErrorMessage != null));
}
```

### 3. **Performance Benchmarks**
```csharp
[Benchmark]
public async Task BenchmarkSequentialExecution()
{
    var executor = new ModuleExecutor();
    var modules = ModuleRegistry.Instance.GetAllModules();
    await executor.ExecuteSequentialAsync(modules);
}

[Benchmark]
public async Task BenchmarkParallelExecution()
{
    var executor = new ModuleExecutor();
    var modules = ModuleRegistry.Instance.GetAllModules();
    await executor.ExecuteParallelAsync(modules);
}
```

---

## Implementation Checklist

### Step 1: Core Module System
- [ ] Create `Modules/` directory
- [ ] Create `IModule.cs` interface
- [ ] Create `ModuleBase.cs` abstract class
- [ ] Create `ModuleResult.cs` record
- [ ] Create `ModuleRegistry.cs`
- [ ] Create `ModuleExecutor.cs`

### Step 2: Convert Existing Code to Modules
- [ ] Create `CpuModule.cs`
- [ ] Create `MemoryModule.cs`
- [ ] Create `GpuModule.cs`
- [ ] Create `OsModule.cs`
- [ ] Create `KernelModule.cs`
- [ ] Create `DiskModule.cs`
- [ ] Create `UptimeModule.cs`
- [ ] Create remaining modules

### Step 3: Configuration System
- [ ] Install `Microsoft.Extensions.Configuration.Json` NuGet package
- [ ] Create `Configuration/` directory
- [ ] Create `SharpFetchConfiguration.cs`
- [ ] Create `ModuleConfiguration.cs`
- [ ] Create `DisplayConfiguration.cs`
- [ ] Create `ConfigurationLoader.cs`
- [ ] Create default `config.json`

### Step 4: CLI Integration
- [ ] Update `Program.cs` with new options
- [ ] Add `--modules` option
- [ ] Add `--config` option
- [ ] Add `--list-modules` option
- [ ] Add `--generate-config` option
- [ ] Add `--format` option

### Step 5: Rendering System
- [ ] Create `Rendering/` directory
- [ ] Create `ModuleResultRenderer.cs`
- [ ] Adapt existing rendering methods
- [ ] Add icon mapping

### Step 6: Testing & Documentation
- [ ] Add unit tests for modules
- [ ] Add integration tests
- [ ] Create user documentation
- [ ] Add performance benchmarks

---

## NuGet Packages Required

```xml
<!-- Add to sharpfetch.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.Configuration.CommandLine" Version="9.0.0" />
  <PackageReference Include="Spectre.Console" Version="0.54.0" />
  <PackageReference Include="System.CommandLine" Version="2.0.5" />
</ItemGroup>
```

---

## Usage Examples (After Implementation)

### ✅ Example 1: Display only CPU and Memory
```bash
sharpfetch --modules cpu,memory
```

### ✅ Example 2: Use custom config file
```bash
sharpfetch --config ~/my-config.json
```

### ✅ Example 3: List all available modules
```bash
sharpfetch --list-modules
```

### ✅ Example 4: Generate default config
```bash
sharpfetch --generate-config config.json
```

### ✅ Example 5: Minimal output without icons
```bash
sharpfetch --format minimal --no-icons
```

### ✅ Example 6: Using config file
```bash
# Create ~/.sharpfetch.json with your preferences
# Then just run:
sharpfetch
```

---

## Benefits of This Architecture

✅ **Modularity**: Easy to add/remove modules without touching core code  
✅ **Performance**: Parallel execution reduces total execution time  
✅ **Flexibility**: Users control what info to display via CLI or config  
✅ **Scalability**: New modules are automatically discovered  
✅ **Testability**: Each module can be unit tested independently  
✅ **Maintainability**: Clean separation of concerns  
✅ **User-Friendly**: JSON config + CLI overrides = great UX  

---

## Next Steps

1. **Review this guide** and ask any questions
2. **Choose which phase to start with** (recommend Phase 1)
3. **Create the basic infrastructure** (interfaces, base classes)
4. **Convert one existing module** as a proof of concept (e.g., CPU)
5. **Test and iterate** before converting all modules
6. **Add configuration support** once module system is stable
7. **Polish and optimize** based on real-world usage

Good luck with your implementation! 🚀
