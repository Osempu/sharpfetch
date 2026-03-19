namespace sharpfetch;

public class MemoryInfo
{
    public long TotalPhysicalMemoryBytes { get; set; }
    public long AvailablePhysicalMemoryBytes { get; set; }
    public long UsedPhysicalMemoryBytes => TotalPhysicalMemoryBytes - AvailablePhysicalMemoryBytes;
    public double MemoryUsagePercentage => TotalPhysicalMemoryBytes > 0
        ? (UsedPhysicalMemoryBytes * 100.0) / TotalPhysicalMemoryBytes
        : 0;

    // Formatted strings for display
    public string TotalMemory => FormatBytes(TotalPhysicalMemoryBytes);
    public string AvailableMemory => FormatBytes(AvailablePhysicalMemoryBytes);
    public string UsedMemory => FormatBytes(UsedPhysicalMemoryBytes);
    public string UsagePercentage => $"{MemoryUsagePercentage:F1}%";

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public override string ToString()
    {
        return $"{UsedMemory} / {TotalMemory} ({UsagePercentage})";
    }
}