namespace sharpfetch.Modules;

/// <summary>Displays free and total space of the primary drive.</summary>
public class DiskModule : ModuleBase
{
    public override string Id => "disk";
    public override string DisplayName => "Disk";
    public override string Description => "Primary drive usage";
    public override string Group => "hardware";
    public override int Order => 40;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => RunAsync(GetDiskInfo, cancellationToken);

    private string GetDiskInfo()
    {
        // Prefer the root/system drive; fall back to any ready drive.
        var drive = DriveInfo.GetDrives()
                        .FirstOrDefault(d => d.IsReady && d.RootDirectory.FullName == "/")
                    ?? DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);

        if (drive is null)
            return "Unknown";

        var free = Helpers.ToGBDouble((ulong)drive.AvailableFreeSpace);
        var total = Helpers.ToGBDouble((ulong)drive.TotalSize);
        return $"{free:F1} GB free / {total:F1} GB";
    }
}
