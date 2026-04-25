namespace sharpfetch.Modules;

/// <summary>A single slice of data to be rendered in a <see cref="Spectre.Console.BreakdownChart"/>.</summary>
public record ChartEntry(string Label, double Value, string Color);

public interface IModule
{
    string Id { get; }
    string DisplayName { get; }
    string Description { get; }
    /// <summary>
    /// The group this module belongs to (e.g. "system", "hardware", "environment", "status").
    /// Used for automatic grouping when no explicit groups are configured.
    /// </summary>
    string Group { get; }
    int Order { get; }
    bool EnabledByDefault { get; }
    Task<ModuleResult> ExecuteAsync(CancellationToken cancellationToken = default);
}

public record ModuleResult
{
    public required string ModuleId { get; init; }
    public required string DisplayName { get; init; }
    /// <summary>Mirrors the originating module's Group value, populated by ModuleBase.</summary>
    public string Group { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Value { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ExecutionTime { get; init; }
    /// <summary>
    /// Optional chart data to display alongside the module value (e.g. memory or disk breakdown).
    /// Null when the module does not support charts.
    /// </summary>
    public IReadOnlyList<ChartEntry>? ChartData { get; init; }

    public static ModuleResult CreateSuccess(
        string moduleId,
        string displayName,
        string value,
        TimeSpan executionTime)
    => new()
    {
        ModuleId = moduleId,
        DisplayName = displayName,
        Success = true,
        Value = value,
        ExecutionTime = executionTime
    };

    public static ModuleResult CreateError(
        string moduleId,
        string displayName,
        string errorMessage,
        TimeSpan executionTime)
    => new()
    {
        ModuleId = moduleId,
        DisplayName = displayName,
        Success = false,
        ErrorMessage = errorMessage,
        ExecutionTime = executionTime
    };
}