namespace sharpfetch.Configuration;

public class SharpFetchConfiguration
{
    public ModuleConfiguration Modules { get; set; } = new();
    public DisplayConfiguration Display { get; set; } = new();
}

public class ModuleConfiguration
{
    /// <summary>
    /// Explicit list of module IDs to run. Empty means all modules enabled by default.
    /// </summary>
    public List<string> Modules { get; set; } = [];
    public bool ParallelExecution { get; set; } = true;
    public int TimeoutMs { get; set; } = 5000;
    public bool ShowExecutionTime { get; set; } = false;
    /// <summary>
    /// Named groups that control how modules are grouped in the output.
    /// When empty, modules are auto-grouped by their built-in Group property.
    /// Only used when <see cref="DisplayConfiguration.GroupModules"/> is <c>true</c>.
    /// </summary>
    public List<GroupConfiguration> Groups { get; set; } = [];
}

/// <summary>
/// Defines a named group of modules for display purposes.
/// </summary>
public class GroupConfiguration
{
    /// <summary>
    /// Matches the module's built-in Group value (e.g. "hardware").
    /// Also used as the fallback display name when DisplayName is empty.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Human-readable title shown in the output panel/tree header.</summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>Spectre.Console color name for the group border/header (e.g. "green", "cyan").</summary>
    public string Color { get; set; } = "green";
    /// <summary>
    /// Explicit list of module IDs to include in this group.
    /// When empty, all enabled modules whose Group matches this Id are included.
    /// </summary>
    public List<string> Modules { get; set; } = [];
    /// <summary>Resolved display name: falls back to a title-cased Id when DisplayName is empty.</summary>
    public string ResolvedDisplayName =>
        string.IsNullOrWhiteSpace(DisplayName) ? ToTitleCase(Id) : DisplayName;

    private static string ToTitleCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpper(value[0]) + value[1..];
}

public class DisplayConfiguration
{
    public string Format { get; set; } = "panels";
    public bool ShowIcons { get; set; } = true;
    /// <summary>
    /// Selects which icon set to use when <see cref="ShowIcons"/> is <c>true</c>.
    /// <see cref="IconStyle.Emoji"/> works in every terminal; <see cref="IconStyle.NerdFont"/>
    /// requires a Nerd Font to be installed and active in the terminal.
    /// </summary>
    public IconStyle IconStyle { get; set; } = IconStyle.Emoji;
    public string ColorScheme { get; set; } = "default";
    public bool ShowCharts { get; set; } = true;
    /// <summary>
    /// When <c>true</c> (default), modules are grouped into named sections in the output.
    /// When <c>false</c>, all modules are rendered in a single flat list regardless of
    /// any groups defined in <see cref="ModuleConfiguration.Groups"/> or the modules' built-in group values.
    /// </summary>
    public bool GroupModules { get; set; } = true;
}
