using sharpfetch.Configuration;
using sharpfetch.Modules;
using Spectre.Console;

namespace sharpfetch.Rendering;

public class ModuleResultRenderer
{
    private readonly SharpFetchConfiguration _config;

    public ModuleResultRenderer(SharpFetchConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Renders results using the configured display format.
    /// Groups are resolved from the results' built-in Group values combined with
    /// any group definitions in the configuration.
    /// </summary>
    public void Render(IReadOnlyList<ModuleResult> results)
    {
        // leftpanel is always flat — it never uses groups
        if (_config.Display.Format.Equals("leftpanel", StringComparison.OrdinalIgnoreCase))
        {
            RenderAsLeftPanel(results.Where(r => r.Success).ToList());
            return;
        }

        var groups = BuildDisplayGroups(results);

        switch (_config.Display.Format.ToLowerInvariant())
        {
            case "panels":
                RenderAsPanels(groups);
                break;
            case "trees":
                RenderAsTrees(groups);
                break;
            case "minimal":
                RenderAsMinimal(groups);
                break;
            default:
                RenderAsMinimal(groups);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Group resolution
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds the ordered list of display groups from the module results.
    /// <list type="bullet">
    ///   <item>When <c>GroupModules</c> is <c>false</c>, all results collapse into one flat group.</item>
    ///   <item>When <c>GroupModules</c> is <c>true</c> and groups are configured, those groups are used.</item>
    ///   <item>When <c>GroupModules</c> is <c>true</c> and no groups are configured, modules are auto-grouped by their built-in Group value.</item>
    /// </list>
    /// </summary>
    private IReadOnlyList<DisplayGroup> BuildDisplayGroups(IReadOnlyList<ModuleResult> results)
    {
        var successful = results.Where(r => r.Success).ToList();

        if (!_config.Display.GroupModules)
            return [new DisplayGroup("System Information", "green", successful)];

        var configuredGroups = _config.Modules.Groups;

        return configuredGroups.Count > 0
            ? BuildConfiguredGroups(successful, configuredGroups)
            : BuildAutoGroups(successful);
    }

    private static IReadOnlyList<DisplayGroup> BuildConfiguredGroups(
        IReadOnlyList<ModuleResult> results,
        IReadOnlyList<GroupConfiguration> groupConfigs)
    {
        var groups = new List<DisplayGroup>();
        var assignedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var gc in groupConfigs)
        {
            IEnumerable<ModuleResult> members;

            if (gc.Modules.Count > 0)
            {
                // Explicit module list in config
                members = gc.Modules
                    .Select(id => results.FirstOrDefault(
                        r => r.ModuleId.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    .Where(r => r != null)
                    .Cast<ModuleResult>();
            }
            else
            {
                // Match by built-in group value stamped onto each result
                members = results.Where(
                    r => r.Group.Equals(gc.Id, StringComparison.OrdinalIgnoreCase));
            }

            var memberList = members.ToList();
            foreach (var r in memberList)
                assignedIds.Add(r.ModuleId);

            if (memberList.Count > 0)
                groups.Add(new DisplayGroup(gc.ResolvedDisplayName, gc.Color, memberList));
        }

        // Unassigned results fall into a catch-all group at the end
        var unassigned = results
            .Where(r => !assignedIds.Contains(r.ModuleId))
            .ToList();

        if (unassigned.Count > 0)
            groups.Add(new DisplayGroup("Other", "grey", unassigned));

        return groups;
    }

    private static IReadOnlyList<DisplayGroup> BuildAutoGroups(IReadOnlyList<ModuleResult> results)
    {
        // Preserve insertion order so groups appear in the order their first member appears
        var groupOrder = new List<string>();
        var groupMap = new Dictionary<string, List<ModuleResult>>(StringComparer.OrdinalIgnoreCase);

        foreach (var result in results)
        {
            if (!groupMap.ContainsKey(result.Group))
            {
                groupOrder.Add(result.Group);
                groupMap[result.Group] = [];
            }
            groupMap[result.Group].Add(result);
        }

        return groupOrder
            .Select(key => new DisplayGroup(
                DisplayName: ToTitleCase(key) + " Info",
                Color: "green",
                Results: groupMap[key]))
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Renderers
    // -------------------------------------------------------------------------

    private void RenderAsPanels(IReadOnlyList<DisplayGroup> groups)
    {
        foreach (var group in groups)
        {
            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn();

            foreach (var result in group.Results)
                grid.AddRow($"[yellow]{FormatLabel(result)}[/]", FormatValue(result));

            AnsiConsole.Write(
                new Panel(grid)
                    .Header(group.DisplayName, Justify.Center)
                    .RoundedBorder()
                    .BorderColor(Color.FromConsoleColor(MapColor(group.Color))));

            // Render BreakdownCharts for any module that provides chart data
            if (_config.Display.ShowCharts)
                RenderCharts(group.Results);
        }
    }

    private void RenderAsTrees(IReadOnlyList<DisplayGroup> groups)
    {
        foreach (var group in groups)
        {
            var tree = new Tree($"[{group.Color} bold]{group.DisplayName}[/]")
                .Style(Style.Parse($"{group.Color} bold"));

            foreach (var result in group.Results)
                tree.AddNode($"[yellow]{FormatLabel(result)}:[/] {FormatValue(result)}");

            AnsiConsole.Write(tree);
        }
    }

    private void RenderAsMinimal(IReadOnlyList<DisplayGroup> groups)
    {
        var first = true;
        foreach (var group in groups)
        {
            if (!first)
                Console.WriteLine();
            first = false;

            AnsiConsole.MarkupLineInterpolated($"[bold]{group.DisplayName}[/]");
            foreach (var result in group.Results)
                Console.WriteLine($"  {FormatLabel(result)}: {result.Value}{FormatExecutionTimeSuffix(result)}");
        }
    }

    private void RenderAsLeftPanel(IReadOnlyList<ModuleResult> results)
    {
        // Left column: labels only, inside a bordered panel
        var labelGrid = new Grid().AddColumn();
        // Right column: values only, plain grid — one empty leading row to
        // align with the label text that sits below the panel's top border line
        var valueGrid = new Grid().AddColumn();
        valueGrid.AddRow(string.Empty);

        foreach (var result in results)
        {
            labelGrid.AddRow($"[yellow]{FormatLabel(result)}[/]");
            valueGrid.AddRow(FormatValue(result));
        }

        var parentGrid = new Grid()
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddRow(
                new Panel(labelGrid)
                    .Header("System Info", Justify.Center)
                    .RoundedBorder()
                    .BorderStyle(new Style(foreground: Color.White))
                    .Padding(0, 0, 1, 0),
                valueGrid);

        AnsiConsole.Write(parentGrid);
    }

    // -------------------------------------------------------------------------
    // Chart rendering
    // -------------------------------------------------------------------------

    /// <summary>
    /// Emits a <see cref="BreakdownChart"/> wrapped in a titled panel for every result
    /// in the group that carries <see cref="ModuleResult.ChartData"/>.
    /// </summary>
    private static void RenderCharts(IReadOnlyList<ModuleResult> results)
    {
        foreach (var result in results)
        {
            if (result.ChartData is not { Count: > 0 })
                continue;

            var chart = new BreakdownChart()
                .FullSize()
                .Width(60)
                .ShowPercentage();

            foreach (var entry in result.ChartData)
                chart.AddItem(entry.Label, entry.Value, MapChartColor(entry.Color));

            AnsiConsole.Write(
                new Panel(chart)
                    .Padding(1, 1)
                    .Header($"{result.DisplayName} Usage"));
        }
    }

    /// <summary>Maps a color name string to a Spectre.Console <see cref="Color"/>.</summary>
    private static Color MapChartColor(string color) => color.ToLowerInvariant() switch
    {
        "red" => Color.Red,
        "yellow" => Color.Yellow,
        "blue" => Color.Blue,
        "cyan" => Color.Aqua,
        "magenta" => Color.Fuchsia,
        "white" => Color.White,
        "grey" or "gray" => Color.Grey,
        _ => Color.Green,
    };

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string FormatLabel(ModuleResult result)
    {
        var icon = GetIconForModule(result.ModuleId);
        return _config.Display.ShowIcons ? $"{icon} {result.DisplayName}" : result.DisplayName;
    }

    /// <summary>
    /// Formats the module value and, when <see cref="ModuleConfiguration.ShowExecutionTime"/> is
    /// enabled, appends a dim timing badge (e.g. <c>[white]foo[/] [grey](1.23 ms)[/]</c>).
    /// </summary>
    private string FormatValue(ModuleResult result)
    {
        var value = $"[white]{result.Value}[/]";
        return _config.Modules.ShowExecutionTime
            ? $"{value} [grey dim]({FormatElapsed(result.ExecutionTime)})[/]"
            : value;
    }

    /// <summary>
    /// Returns a plain-text timing suffix for renderers that write via <see cref="Console.WriteLine"/>
    /// rather than Spectre markup (e.g. the minimal renderer).
    /// </summary>
    private string FormatExecutionTimeSuffix(ModuleResult result) =>
        _config.Modules.ShowExecutionTime
            ? $" ({FormatElapsed(result.ExecutionTime)})"
            : string.Empty;

    /// <summary>Formats a <see cref="TimeSpan"/> as a compact human-readable string.</summary>
    private static string FormatElapsed(TimeSpan elapsed) =>
        elapsed.TotalMilliseconds < 1
            ? $"{elapsed.TotalMicroseconds:F0} µs"
            : elapsed.TotalSeconds >= 1
                ? $"{elapsed.TotalSeconds:F2} s"
                : $"{elapsed.TotalMilliseconds:F2} ms";

    private static string GetIconForModule(string moduleId) => moduleId switch
    {
        "user" => "👤",
        "os" => "🖥️",
        "kernel" => "✅",
        "cpu" => "🔲",
        "memory" => "💾",
        "gpu" => "🎮",
        "bios" => "🔧",
        "disk" => "💿",
        "shell" => "🐚",
        "terminal" => "📟",
        "windowmanager" => "🪟",
        "uptime" => "🔥",
        "datetime" => "🕐",
        _ => "•",
    };

    /// <summary>Maps a color name string to a <see cref="ConsoleColor"/> for Spectre.Console.</summary>
    private static ConsoleColor MapColor(string color) => color.ToLowerInvariant() switch
    {
        "red" => ConsoleColor.Red,
        "yellow" => ConsoleColor.Yellow,
        "blue" => ConsoleColor.Blue,
        "cyan" => ConsoleColor.Cyan,
        "magenta" => ConsoleColor.Magenta,
        "white" => ConsoleColor.White,
        "grey" or "gray" => ConsoleColor.Gray,
        _ => ConsoleColor.Green,
    };

    private static string ToTitleCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpper(value[0]) + value[1..];

    // -------------------------------------------------------------------------
    // Internal model
    // -------------------------------------------------------------------------

    private record DisplayGroup(string DisplayName, string Color, IReadOnlyList<ModuleResult> Results);
}
