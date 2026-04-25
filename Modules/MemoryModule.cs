namespace sharpfetch.Modules;

public class MemoryModule : ModuleBase
{
    public override string Id => "memory";
    public override string DisplayName => "Memory";
    public override string Description => "RAM usage information";
    public override string Group => "hardware";
    public override int Order => 25;

    // Cached so GetChartData() uses the same values as GetValueAsync()
    private MemoryInfo? _memoryInfo;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(() =>
        {
            _memoryInfo = new Sysinfo().MemoryInfo;
            return _memoryInfo.ToString();
        }, cancellationToken);
    }

    protected override IReadOnlyList<ChartEntry>? GetChartData()
    {
        if (_memoryInfo is null)
            return null;

        return
        [
            new ChartEntry("Used", Helpers.ToGBDouble((ulong)_memoryInfo.UsedPhysicalMemoryBytes), "red"),
            new ChartEntry("Free", Helpers.ToGBDouble((ulong)_memoryInfo.AvailablePhysicalMemoryBytes), "green"),
        ];
    }
}