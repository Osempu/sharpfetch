namespace sharpfetch.Modules;

public class OsModule : ModuleBase
{
    public override string Id => "os";
    public override string DisplayName => "OS";
    public override string Description => "Operating System Information";
    public override int Order => 5;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return FromResult(Sysinfo.GetOSName());
    }
}