using sharpfetch.Configuration;
using sharpfetch.Modules;
using Spectre.Console;

namespace sharpfetch.Rendering;

public class ModuleResultRenderer
{
    private readonly DisplayConfiguration _config;

    public ModuleResultRenderer(DisplayConfiguration config)
    {
        _config = config;
    }

    public void Renderer(IReadOnlyList<ModuleResult> results)
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