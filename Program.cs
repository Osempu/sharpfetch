using System.CommandLine;
using sharpfetch.Configuration;
using sharpfetch.Modules;
using sharpfetch.Rendering;
using Spectre.Console;

var rootCommand = new RootCommand("SharpFetch - Fast System Information Tool");

var configOption = new Option<string?>("--configuration", "-c")
{
    Description = "Pahth to custom configuration file"
};

var modulesoption = new Option<string[]?>("--modules", "-m")
{
    Description = "Comma-separated list of modules to display (e.g., cpu, memory, gpu)",
    Arity = ArgumentArity.OneOrMore,
    AllowMultipleArgumentsPerToken = true
};

var formatOption = new Option<string?>("--format", "-f")
{
    Description = "Output format: panels, trees, minimnal, leftpanel"
};

var noInconsOption = new Option<bool>("--no-icons")
{
    Description = "Disable icons in output"
};

var showChartsOption = new Option<bool?>("--show-charts")
{
    Description = "Show breakdown charts (true/false). Overrides config ShowCharts when specified."
};

var groupOption = new Option<bool?>("--group")
{
    Description = "Group modules into sections (true/false). Overrides config GroupModules when specified."
};

var parallelOption = new Option<bool>("--parallel")
{
    DefaultValueFactory = isParallel => true,
    Description = "Execute modules in parallel"
};

var listModulesOption = new Option<bool>("--list-modules")
{
    Description = "List all available modules"
};

var generateConfigOption = new Option<string?>("--generate-config")
{
    Description = "Generate a default configuration file at the specified path"
};

// Add options to root command
rootCommand.Options.Add(configOption);
rootCommand.Options.Add(modulesoption);
rootCommand.Options.Add(formatOption);
rootCommand.Options.Add(noInconsOption);
rootCommand.Options.Add(showChartsOption);
rootCommand.Options.Add(groupOption);
rootCommand.Options.Add(parallelOption);
rootCommand.Options.Add(listModulesOption);
rootCommand.Options.Add(generateConfigOption);

rootCommand.SetAction(async (context) =>
{
    var configPath = context.GetValue(configOption);
    var modules = context.GetValue(modulesoption);
    var format = context.GetValue(formatOption);
    var noIcons = context.GetValue(noInconsOption);
    var showCharts = context.GetValue(showChartsOption);
    var group = context.GetValue(groupOption);
    var parallel = context.GetValue(parallelOption);
    var listModules = context.GetValue(listModulesOption);
    var generateConfig = context.GetValue(generateConfigOption);

    if (!string.IsNullOrEmpty(generateConfig))
    {
        ConfigurationLoader.CreateDefaultConfigFile(generateConfig);
        Console.WriteLine($"✓ Default configuration file created at: {generateConfig}");
        return;
    }

    if (listModules)
    {
        var registry = ModuleRegistry.Instance;
        AnsiConsole.MarkupLine("[bold red]Available Modules[/]");
        foreach (var module in registry.GetAllModules())
        {
            var status = module.EnabledByDefault ? "[enabled]" : "[disabled]";
            AnsiConsole.MarkupLineInterpolated($"  {module.Id,-15} {status,-12} - {module.Description}");
        }
        return;
    }

    var config = ConfigurationLoader
        .Load(
            context.Tokens.Select(t => t.Value).ToArray(),
            configPath);

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

    // CLI flags take precedence over config file values when explicitly provided
    if (showCharts.HasValue)
    {
        config.Display.ShowCharts = showCharts.Value;
    }

    if (group.HasValue)
    {
        config.Display.GroupModules = group.Value;
    }

    config.Modules.ParallelExecution = parallel;

    var executor = new ModuleExecutor();
    var results = await executor.ExecuteFromConfigAsync(config.Modules);

    var renderer = new ModuleResultRenderer(config);
    renderer.Render(results);
});

return rootCommand.Parse(args).Invoke();

