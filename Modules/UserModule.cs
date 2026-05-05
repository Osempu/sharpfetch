namespace sharpfetch.Modules;

/// <summary>Displays the current user and machine name (user@hostname).</summary>
public class UserModule : ModuleBase
{
    public override string Id => "user";
    public override string DisplayName => "User";
    public override string Description => "Current user and machine name";
    public override string Group => "system";
    public override int Order => 5;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => FromResult($"{Environment.UserName}@{Environment.MachineName}");
}
