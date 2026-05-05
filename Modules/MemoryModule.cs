using System.Runtime.InteropServices;

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
            _memoryInfo = GetMemory();
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

    // -------------------------------------------------------------------------
    // Memory detection
    // -------------------------------------------------------------------------

    private static MemoryInfo GetMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetWindowsMemoryInfo();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetLinuxMemoryInfo();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetMacOSMemoryInfo();

        return GetFallbackMemoryInfo();
    }

    private static MemoryInfo GetWindowsMemoryInfo()
    {
        var memStatus = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(memStatus))
        {
            return new MemoryInfo
            {
                TotalPhysicalMemoryBytes = (long)memStatus.ullTotalPhys,
                AvailablePhysicalMemoryBytes = (long)memStatus.ullAvailPhys
            };
        }

        return GetFallbackMemoryInfo();
    }

    private static MemoryInfo GetLinuxMemoryInfo()
    {
        long total = 0, available = 0;

        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (line.StartsWith("MemTotal:"))
                total = Helpers.ParseMemInfoLine(line) * 1024; // KB → bytes
            else if (line.StartsWith("MemAvailable:"))
            {
                available = Helpers.ParseMemInfoLine(line) * 1024;
                break;
            }
        }

        return new MemoryInfo
        {
            TotalPhysicalMemoryBytes = total,
            AvailablePhysicalMemoryBytes = available
        };
    }

    private static MemoryInfo GetMacOSMemoryInfo()
    {
        long totalBytes = long.Parse(Helpers.Execute("sysctl", "-n hw.memsize").Trim());

        string vmStat = Helpers.Execute("vm_stat", "");
        long pagesFree = Helpers.ParseVmStat(vmStat, "Pages free:");
        long pagesInactive = Helpers.ParseVmStat(vmStat, "Pages inactive:");
        const long pageSize = 4096;

        return new MemoryInfo
        {
            TotalPhysicalMemoryBytes = totalBytes,
            AvailablePhysicalMemoryBytes = (pagesFree + pagesInactive) * pageSize
        };
    }

    private static MemoryInfo GetFallbackMemoryInfo()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();
            long totalBytes = gcInfo.TotalAvailableMemoryBytes;
            long usedBytes = gcInfo.MemoryLoadBytes;
            long availableBytes = Math.Max(0, totalBytes - usedBytes);

            return new MemoryInfo
            {
                TotalPhysicalMemoryBytes = totalBytes,
                AvailablePhysicalMemoryBytes = availableBytes
            };
        }
        catch
        {
            return new MemoryInfo
            {
                TotalPhysicalMemoryBytes = 0,
                AvailablePhysicalMemoryBytes = 0
            };
        }
    }

    // -------------------------------------------------------------------------
    // Windows P/Invoke — private to this module
    // -------------------------------------------------------------------------

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }
}
