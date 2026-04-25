namespace sharpfetch.Modules;

public class MemoryModule : ModuleBase
{
    public override string Id => "memory";
    public override string DisplayName => "Memory";
    public override string Description => "RAM usage information";
    public override string Group => "hardware";
    public override int Order => 25;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(GetMemoryInfo, cancellationToken);
    }

    private string GetMemoryInfo()
    {
        var sysInfo = new Sysinfo();
        return sysInfo.MemoryInfo.ToString();
    }
}