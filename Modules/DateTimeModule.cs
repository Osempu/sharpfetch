namespace sharpfetch.Modules;

/// <summary>Displays the current local date and time.</summary>
public class DateTimeModule : ModuleBase
{
    public override string Id => "datetime";
    public override string DisplayName => "Date / Time";
    public override string Description => "Current local date and time";
    public override string Group => "status";
    public override int Order => 80;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => FromResult(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
}
