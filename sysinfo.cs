using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace sharpfetch;

public class Sysinfo
{
    public string User { get; set; } = Environment.UserName;
    public string Machine { get; set; } = Environment.MachineName;
    public string OsDescription { get; set; } = RuntimeInformation.OSDescription;
    public string OsVersion { get; set; } = Environment.OSVersion.Version.ToString();
    public string OS { get; set; } = GetOSName();
    public string Arch { get; set; } = RuntimeInformation.OSArchitecture.ToString();
    public string CPU => GetCpu();
    public MemoryInfo MemoryInfo => GetMemory();
    public string Uptime => GetUptime();
    public string DiskInfo => GetDisk();

    public static string GetOSName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "Windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "Linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Mac OS";
        }
        else
        {
            return "Unknown OS";
        }
    }

    private string GetCpu()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER", EnvironmentVariableTarget.Machine) ?? "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var line = File.ReadLines("/proc/cpuinfo").FirstOrDefault(l => l.StartsWith("model name"));
            return line?.Split(':').Last().Trim() ?? "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Helpers.Execute("sysctl", "-n machdep.cpu.brand_string");
        }
        return "Unknown";
    }

    private MemoryInfo GetMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsMemoryInfo();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxMemoryInfo();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOSMemoryInfo();
        }

        return GetFallbackMemoryInfo();
    }

    private string GetUptime()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return Helpers.FormatTime(uptime);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var uptimeSeconds = double.Parse(File.ReadAllText("/proc/uptime").Split(' ')[0]);
            return Helpers.FormatTime(TimeSpan.FromSeconds(uptimeSeconds));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var bootTime = Helpers.Execute("sysctl", "-n kern.boottime");
        }

        return "Unknown";
    }

    private string GetDisk()
    {
        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.RootDirectory.FullName == "/")
            ?? DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);

        if (drive != null)
        {
            var free = Helpers.ToGB((ulong)drive.AvailableFreeSpace);
            var total = Helpers.ToGB((ulong)drive.TotalSize);
            return $"{free} GB free / {total} GB";
        }

        return "Unknown";
    }

    private MemoryInfo GetWindowsMemoryInfo()
    {
        var gcInfo = GC.GetGCMemoryInfo();
        long totalBytes = gcInfo.TotalAvailableMemoryBytes;
        long usedBytes = gcInfo.MemoryLoadBytes;
        long availableBytes = totalBytes - usedBytes;

        return new MemoryInfo
        {
            TotalPhysicalMemoryBytes = totalBytes,
            AvailablePhysicalMemoryBytes = availableBytes
        };
    }

    private MemoryInfo GetLinuxMemoryInfo()
    {
        long total = 0, available = 0;

        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (line.StartsWith("MemTotal:"))
            {
                total = Helpers.ParseMemInfoLine(line) * 1024; // Convert KB to bytes
            }
            else if (line.StartsWith("MemAvailable:"))
            {
                available = Helpers.ParseMemInfoLine(line) * 1024;
                break; // We have what we need
            }
        }

        return new MemoryInfo
        {
            TotalPhysicalMemoryBytes = total,
            AvailablePhysicalMemoryBytes = available
        };
    }

    private MemoryInfo GetMacOSMemoryInfo()
    {
        long totalBytes = long.Parse(Helpers.Execute("sysctl", "-n hw.memsize").Trim());

        // Get page statistics for available memory calculation
        string vmStat = Helpers.Execute("vm_stat", "");
        long pagesFree = Helpers.ParseVmStat(vmStat, "Pages free:");
        long pagesInactive = Helpers.ParseVmStat(vmStat, "Pages inactive:");
        long pageSize = 4096; // Standard page size

        long availableBytes = (pagesFree + pagesInactive) * pageSize;

        return new MemoryInfo
        {
            TotalPhysicalMemoryBytes = totalBytes,
            AvailablePhysicalMemoryBytes = availableBytes
        };
    }

    private MemoryInfo GetFallbackMemoryInfo()
    {
        try
        {
            var gcInfo = GC.GetGCMemoryInfo();

            // TotalAvailableMemoryBytes represents total physical memory
            // (or container limit if running in a container)
            long totalBytes = gcInfo.TotalAvailableMemoryBytes;

            // MemoryLoadBytes represents current memory usage
            long usedBytes = gcInfo.MemoryLoadBytes;

            // Calculate available memory
            long availableBytes = totalBytes - usedBytes;

            // Ensure non-negative values
            if (availableBytes < 0)
            {
                availableBytes = 0;
            }

            return new MemoryInfo
            {
                TotalPhysicalMemoryBytes = totalBytes,
                AvailablePhysicalMemoryBytes = availableBytes
            };
        }
        catch
        {
            // If even GC.GetGCMemoryInfo() fails, return zeros
            return new MemoryInfo
            {
                TotalPhysicalMemoryBytes = 0,
                AvailablePhysicalMemoryBytes = 0
            };
        }
    }
}