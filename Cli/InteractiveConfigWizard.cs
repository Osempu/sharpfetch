using sharpfetch.Configuration;
using Spectre.Console;

namespace sharpfetch;

public static class InteractiveConfigWizard
{
    public static void Run()
    {
        var config = new SharpFetchConfiguration();

        AnsiConsole.Write(new FigletText("Sharpfetch Config").Centered().Color(Color.Green));

        config.Modules.Modules = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [green]Modules[/] to enable:")
                .AddChoices("User", "OS", "Disk", "Memory", "CPU", "GPU",
                            "Network", "Battery", "Audio", "Peripherals"))
                            .Select(m => m.ToLower()).ToList();

        config.Display.Format = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [green]Format[/] to use:")
                .AddChoices("Minimal", "Panels", "Trees", "Left Panel"));

        config.Display.ShowIcons = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Show [green]Icons[/]?")
                .AddChoices(true, false));

        if (config.Display.ShowIcons)
        {
            config.Display.IconStyle = AnsiConsole.Prompt(
                new SelectionPrompt<IconStyle>()
                    .Title("Select [green]Icon Style[/]:")
                    .AddChoices(IconStyle.Emoji, IconStyle.NerdFont)
                    .UseConverter(style => style switch
                    {
                        IconStyle.NerdFont => "Nerd Font  (requires a Nerd Font installed in your terminal)",
                        _                  => "Emoji      (works in every terminal, no setup needed)"
                    }));
        }

        config.Display.ShowCharts = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Show [green]Charts[/]?")
                .AddChoices(true, false));

        config.Display.GroupModules = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Group [green]Modules[/]?")
                .AddChoices(true, false));

        config.Modules.ParallelExecution = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Enable [green]Parallel Execution[/]?")
                .AddChoices(true, false));

        config.Modules.ShowExecutionTime = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Show [green]Execution Time[/]?")
                .AddChoices(true, false));

        PrintConfigPreview(config);
    }

    private static void PrintConfigPreview(SharpFetchConfiguration config)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn().PadRight(1));
        grid.AddColumn(new GridColumn().PadLeft(1));

        grid.AddRow("[green]Current Configuration:[/]");
        grid.AddRow($"📦 [yellow]Modules:[/] {string.Join(", ", config.Modules.Modules)}");
        grid.AddRow($"🖼️ [yellow]Format:[/] {config.Display.Format}");
        grid.AddRow($"🔣 [yellow]Show Icons:[/] {config.Display.ShowIcons}");
        if (config.Display.ShowIcons)
            grid.AddRow($"🎨 [yellow]Icon Style:[/] {config.Display.IconStyle}");
        grid.AddRow($"📊 [yellow]Show Charts:[/] {config.Display.ShowCharts}");
        grid.AddRow($"📂 [yellow]Group Modules:[/] {config.Display.GroupModules}");
        grid.AddRow($"🏃🏼 [yellow]Parallel Execution:[/] {config.Modules.ParallelExecution}");
        grid.AddRow($"⏱️ [yellow]Show Execution Time:[/] {config.Modules.ShowExecutionTime}");

        AnsiConsole.Write(grid);

        if (AnsiConsole.Confirm("Save this configuration to [green]config.json[/]?"))
        {
            // ConfigurationLoader.Save(config, "config.json");
            ConfigurationLoader.CreateDefaultConfigFile(config);
            AnsiConsole.MarkupLine("[green]✅ Configuration saved to config.json[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]❌ Configuration not saved[/]");
        }
    }
}
