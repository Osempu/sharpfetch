namespace sharpfetch.Modules;

public class GpuModule : ModuleBase
{
    public override string Id => "gpu";
    public override string DisplayName => "GPU";
    public override string Description => "Graphics Card Information";
    public override int Order => 30;
    public override bool EnabledByDefault => true;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(GetCpuInfo, cancellationToken);
    }

    private string GetCpuInfo()
    {
        var sysinfo = new Sysinfo();
        return sysinfo.GPU;
    }
}