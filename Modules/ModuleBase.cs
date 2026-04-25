using sharpfetch.Modules;

public abstract class ModuleBase : IModule
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public virtual string Description => string.Empty;
    /// <summary>
    /// Override in each module to declare its built-in group.
    /// Valid built-in values: "system", "hardware", "environment", "status".
    /// </summary>
    public virtual string Group => "general";
    public virtual int Order => 100;
    public virtual bool EnabledByDefault => true;

    public async Task<ModuleResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var value = await GetValueAsync(cancellationToken);
            stopwatch.Stop();

            return ModuleResult.CreateSuccess(
                Id,
                DisplayName,
                value,
                stopwatch.Elapsed) with { Group = Group, ChartData = GetChartData() };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return ModuleResult.CreateError(
                Id,
                DisplayName,
                ex.Message,
                stopwatch.Elapsed) with { Group = Group };
        }
    }

    protected abstract Task<string> GetValueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Override in modules that support chart rendering (e.g. memory, disk).
    /// Returns null by default — no chart is rendered.
    /// </summary>
    protected virtual IReadOnlyList<ChartEntry>? GetChartData() => null;

    protected Task<string> FromResult(string value) => Task.FromResult(value);

    protected async Task<string> RunAsync(Func<string> operation, CancellationToken cancellationToken)
    {
        return await Task.Run(operation, cancellationToken);
    }
}
