# SharpFetch

A cross-platform system information CLI tool built with .NET 9 and C#, inspired by [fastfetch](https://github.com/fastfetch-cli/fastfetch). SharpFetch displays your system specs in a clean, configurable terminal UI powered by [Spectre.Console](https://spectreconsole.net/).

```
╭─ System Info ─────────────────╮  ╭─ Hardware Info ──────────────────────────────────────────────╮
│ 👤 User    oscar@OSCAR-PC      │  │ 🖥️  CPU    Intel64 Family 6 Model 183 Stepping 1, GenuineIntel │
│ 🖥️  OS      Windows             │  │ 🧠 Memory  14.07 GB / 31.78 GB (44.3%)                        │
│ 💾 Disk    452 GB free / 953 GB│  │ 🎮 GPU     NVIDIA GeForce RTX 4070 Ti SUPER                    │
╰───────────────────────────────╯  ╰──────────────────────────────────────────────────────────────╯
```

---

## Features

- **Modular architecture** — each system info category is an independent, self-contained module
- **Multiple output formats** — `panels`, `trees`, `minimal`, `leftpanel`
- **Icon support** — emoji (works everywhere) or Nerd Font glyphs (for patched fonts)
- **Breakdown charts** — optional visual bars for memory and disk usage
- **Module grouping** — organise output into logical sections (System, Hardware, Environment, Status)
- **Parallel execution** — gather all module data concurrently for fast startup
- **JSON configuration** — persistent settings via `config.json`, all overridable via CLI flags
- **Interactive config wizard** — guided setup with `sharpfetch config-wiz`
- **Cross-platform** — Windows, Linux, and macOS

---

## Modules

| ID | Display Name | Group | Description |
|----|-------------|-------|-------------|
| `user` | User | system | Current user and hostname |
| `os` | OS | system | Operating system name |
| `kernel` | Kernel | system | Kernel / OS version |
| `uptime` | Uptime | status | System uptime |
| `datetime` | Date/Time | status | Current date and time |
| `cpu` | CPU | hardware | Processor model |
| `memory` | Memory | hardware | RAM usage |
| `gpu` | GPU | hardware | Graphics card(s) |
| `disk` | Disk | hardware | Primary disk usage |
| `bios` | BIOS | hardware | BIOS/firmware version |
| `shell` | Shell | environment | Active shell |
| `terminal` | Terminal | environment | Active terminal emulator |
| `wm` | Window Manager | environment | Desktop/window manager |

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run from source

```bash
git clone https://github.com/your-username/sharpfetch.git
cd sharpfetch
dotnet run
```

### Build a release binary

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

The compiled binary will be in `bin/Release/net9.0/<runtime>/publish/`.

---

## Usage

```
sharpfetch [OPTIONS] [SUBCOMMAND]
```

### Options

| Flag | Description |
|------|-------------|
| `-c, --configuration <path>` | Path to a custom config file |
| `-m, --modules <ids...>` | Modules to display, e.g. `cpu memory gpu` |
| `-f, --format <name>` | Output format: `panels`, `trees`, `minimal`, `leftpanel` |
| `--no-icons` | Disable icons |
| `--icon-style <style>` | `Emoji` or `NerdFont` |
| `--show-charts <bool>` | Show breakdown charts |
| `--group <bool>` | Group modules into sections |
| `--parallel` | Execute modules in parallel (default: on) |
| `--list-modules` | List all available modules |
| `--generate-config <path>` | Write a default `config.json` to the given path |

### Subcommands

| Command | Description |
|---------|-------------|
| `config-wiz` | Launch the interactive configuration wizard |

### Examples

```bash
# Default output (reads config.json)
sharpfetch

# Only show CPU and memory, in minimal format
sharpfetch --modules cpu memory --format minimal

# Use Nerd Font icons with charts
sharpfetch --icon-style NerdFont --show-charts true

# Generate a starter config file
sharpfetch --generate-config ~/.config/sharpfetch/config.json

# Interactive setup
sharpfetch config-wiz
```

---

## Configuration

SharpFetch reads `config.json` from the working directory. Generate a default file with:

```bash
sharpfetch --generate-config config.json
```

### Schema

```jsonc
{
  "Modules": {
    "Modules": ["user", "os", "disk", "memory", "cpu", "gpu"],  // active modules
    "ParallelExecution": true,   // run modules concurrently
    "TimeoutMs": 5000,           // per-module timeout
    "ShowExecutionTime": false   // append timing to each label
  },
  "Display": {
    "Format": "Trees",           // panels | trees | minimal | leftpanel
    "ShowIcons": true,
    "IconStyle": "Emoji",        // Emoji | NerdFont
    "ShowCharts": false,         // breakdown charts for memory/disk
    "GroupModules": true         // group by hardware/system/environment/status
  }
}
```

**CLI flags always override config file values** when explicitly provided.

---

## Project Structure

```
sharpfetch/
├── Program.cs                   # Entry point; CLI argument definitions
│
├── Modules/                     # One file per system info module
│   ├── IModule.cs               # Module contract
│   ├── ModuleBase.cs            # Base class with shared helpers
│   ├── ModuleExecutor.cs        # Runs modules (parallel or sequential)
│   ├── ModulesRegistry.cs       # Auto-discovers all registered modules
│   ├── CpuModule.cs
│   ├── MemoryModule.cs
│   ├── GpuModule.cs
│   └── ...                      # One file per module
│
├── Configuration/               # Config schema and loading
│   ├── SharpFetchConfiguration.cs
│   ├── ConfigurationLoader.cs
│   └── IconStyle.cs             # Emoji / NerdFont enum
│
├── Rendering/                   # Terminal output
│   ├── ModuleResultRenderer.cs  # Formats and prints module results
│   └── ModuleIcons.cs           # Icon dictionaries (emoji + nerd font)
│
├── Utilities/                   # Shared helpers and models
│   ├── Helpers.cs               # Execute(), FormatTime(), ToGB(), etc.
│   └── MemoryInfo.cs            # Memory data model
│
├── Cli/
│   └── InteractiveConfigWizard.cs  # config-wiz subcommand
│
└── config.json                  # Default configuration file
```

---

## Contributing

Contributions are welcome. The most common tasks are adding a new module or a new output format — both follow a straightforward pattern.

### Adding a new module

1. Create `Modules/YourModule.cs` inheriting from `ModuleBase`:

```csharp
namespace sharpfetch.Modules;

public class YourModule : ModuleBase
{
    public override string Id          => "yourmodule";
    public override string DisplayName => "Your Module";
    public override string Description => "What it shows";
    public override string Group       => "system"; // system | hardware | environment | status
    public override int    Order       => 50;       // controls display order

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(() =>
        {
            // collect and return the info string
            return "your value";
        }, cancellationToken);
    }
}
```

2. Register it in `ModulesRegistry.cs`.
3. Add an icon entry in `Rendering/ModuleIcons.cs` (emoji and Nerd Font variants).
4. Add it to `config.json` `Modules` array to enable it by default.

### Adding a new output format

1. Add a new render path in `Rendering/ModuleResultRenderer.cs`.
2. Handle the new format name in the `Render()` switch.
3. Add the name as an accepted value in the `--format` option description in `Program.cs`.

### Code style

- `.NET 9` / `C# 13`, file-scoped namespaces, nullable reference types enabled
- One class per file, filename matches class name (lowercase)
- Platform-specific code uses `RuntimeInformation.IsOSPlatform()` guards
- Graceful degradation: always return `"Unknown"` on failure — never throw from a module
- Keep P/Invoke declarations `private` and scoped to the module that needs them

### Building and testing

```bash
dotnet build          # debug build
dotnet build -c Release
dotnet run            # run with default config
dotnet run -- --list-modules
```

> There is no test project yet. When adding tests, use xUnit and run with `dotnet test`.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| [Spectre.Console](https://spectreconsole.net/) | 0.54.0 | Rich terminal UI (panels, trees, charts, prompts) |
| [System.CommandLine](https://github.com/dotnet/command-line-api) | 2.0.5 | CLI argument parsing |
| Microsoft.Extensions.Configuration | 9.0.x | JSON config loading |

---

## Roadmap

- [ ] Unit tests (xUnit)
- [ ] Native AOT support for faster startup and single-file publish
- [ ] Module selection via `config.json` enable/disable flags
- [ ] Color scheme customisation
- [ ] Network info module
- [ ] Battery module
- [ ] `--watch` mode for live-updating output

---

## License

MIT
