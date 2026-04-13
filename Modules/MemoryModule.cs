namespace sharpfetch.Modules;

public class MemoryModule : ModuleBase
{
    public override string Id => "memory";
    public override string DisplayName => "Memory";
    public override string Description => "Ram usage information";
    public override int Order => 20;

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