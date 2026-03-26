using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace sharpfetch;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal class MEMORYSTATUSEX
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

public partial class Sysinfo
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

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
    public string Kernel => GetKernel();
    public string WindowManager => GetWindowManager();
    public string Terminal => GetTerminal();
    public string Shell => GetShell();
    public string Bios => GetBios();
    public string GPU => GetGpu();
    public string DateTime => GetDateTime();

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
        var memStatus = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(memStatus))
        {
            return new MemoryInfo
            {
                TotalPhysicalMemoryBytes = (long)memStatus.ullTotalPhys,
                AvailablePhysicalMemoryBytes = (long)memStatus.ullAvailPhys
            };
        }

        // Fallback to GC info if the API call fails
        return GetFallbackMemoryInfo();
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

    private string GetKernel()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows NT kernel version
            return Environment.OSVersion.Version.ToString();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Helpers.Execute("uname", "-r");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Helpers.Execute("uname", "-r");
        }

        return "Unknown";
    }

    private string GetWindowManager()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows uses DWM (Desktop Window Manager)
            return "Desktop Window Manager";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Try to detect common window managers
            string[] wmEnvVars = { "XDG_CURRENT_DESKTOP", "DESKTOP_SESSION", "GDMSESSION" };
            
            foreach (var envVar in wmEnvVars)
            {
                var value = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            // Try to check running processes for common WMs
            try
            {
                var wmCheck = Helpers.Execute("ps", "-e");
                string[] wms = { "gnome-shell", "kwin", "xfwm4", "i3", "awesome", "dwm", "bspwm" };
                
                foreach (var wm in wms)
                {
                    if (wmCheck.Contains(wm))
                    {
                        return wm;
                    }
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Quartz Compositor";
        }

        return "Unknown";
    }

    private string GetTerminal()
    {
        // Check common terminal environment variables
        var term = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (!string.IsNullOrWhiteSpace(term))
        {
            return term;
        }

        term = Environment.GetEnvironmentVariable("TERMINAL_EMULATOR");
        if (!string.IsNullOrWhiteSpace(term))
        {
            return term;
        }

        // On Windows, detect common terminals
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var wtSession = Environment.GetEnvironmentVariable("WT_SESSION");
            if (!string.IsNullOrWhiteSpace(wtSession))
            {
                return "Windows Terminal";
            }

            var psModulePath = Environment.GetEnvironmentVariable("PSModulePath");
            if (psModulePath?.Contains("WindowsPowerShell") == true)
            {
                return "Windows Console Host";
            }

            // Check if running in VS Code terminal
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VSCODE_GIT_IPC_HANDLE")))
            {
                return "VS Code";
            }

            return "cmd.exe";
        }

        // For Linux/macOS, check the terminal name
        var termName = Environment.GetEnvironmentVariable("TERM");
        if (!string.IsNullOrWhiteSpace(termName))
        {
            return termName;
        }

        return "Unknown";
    }

    private string GetShell()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Check if running PowerShell
            var psVersion = Environment.GetEnvironmentVariable("PSModulePath");
            if (!string.IsNullOrWhiteSpace(psVersion))
            {
                var isPwsh = psVersion.Contains("PowerShell\\7") || psVersion.Contains("PowerShell/7");
                return isPwsh ? "PowerShell Core" : "Windows PowerShell";
            }

            return "cmd.exe";
        }
        else
        {
            // Unix-like systems
            var shell = Environment.GetEnvironmentVariable("SHELL");
            if (!string.IsNullOrWhiteSpace(shell))
            {
                // Extract just the shell name from the path
                return Path.GetFileName(shell);
            }

            return "Unknown";
        }
    }

    private string GetBios()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // Use PowerShell CIM cmdlet (modern replacement for WMI)
                var biosVersion = Helpers.Execute("powershell", "-NoProfile -Command \"Get-CimInstance -ClassName Win32_BIOS | Select-Object -ExpandProperty SMBIOSBIOSVersion\"");
                
                if (!string.IsNullOrWhiteSpace(biosVersion))
                {
                    return biosVersion;
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                // Try to read from DMI
                if (File.Exists("/sys/class/dmi/id/bios_version"))
                {
                    return File.ReadAllText("/sys/class/dmi/id/bios_version").Trim();
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                return Helpers.Execute("system_profiler", "SPHardwareDataType | grep 'Boot ROM Version'").Split(':').Last().Trim();
            }
            catch { }

            return "Unknown";
        }

        return "Unknown";
    }

    private string GetGpu()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // Use PowerShell CIM cmdlet (modern replacement for WMI)
                var gpuInfo = Helpers.Execute("powershell", "-NoProfile -Command \"Get-CimInstance -ClassName Win32_VideoController | Select-Object -ExpandProperty Name\"");
                
                if (!string.IsNullOrWhiteSpace(gpuInfo))
                {
                    var lines = gpuInfo.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    
                    if (lines.Length > 0)
                    {
                        return string.Join(", ", lines);
                    }
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                // Try lspci for GPU info
                var lspci = Helpers.Execute("lspci", "");
                var gpuLines = lspci.Split('\n')
                    .Where(l => l.Contains("VGA compatible controller") || l.Contains("3D controller"))
                    .ToList();

                if (gpuLines.Count > 0)
                {
                    var gpus = gpuLines.Select(line =>
                    {
                        var parts = line.Split(':');
                        return parts.Length > 2 ? parts[2].Trim() : "Unknown";
                    }).ToList();

                    return string.Join(", ", gpus);
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var gpuInfo = Helpers.Execute("system_profiler", "SPDisplaysDataType");
                var lines = gpuInfo.Split('\n');
                
                foreach (var line in lines)
                {
                    if (line.Contains("Chipset Model:"))
                    {
                        return line.Split(':').Last().Trim();
                    }
                }
            }
            catch { }

            return "Unknown";
        }

        return "Unknown";
    }

    private string GetDateTime()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}