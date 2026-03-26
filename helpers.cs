using System.Diagnostics;

namespace sharpfetch;

public static class Helpers
{
    public static object ToGB(ulong bytes)
    => bytes / 1024 / 1024 / 1024;

    public static string FormatTime(TimeSpan uptime)
    {
        return $"{(int)uptime.TotalHours}h {uptime.Minutes}m {uptime.Seconds}s";
    }

    public static string Execute(string cmd, string args)
    {
        try
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.Start();
            var output = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            return output;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static long ParseMemInfoLine(string line)
    {
        // Extract number from "MemTotal:      16384000 kB"
        var parts = line.Split(':', StringSplitOptions.TrimEntries);
        var numPart = parts[1].Split(' ')[0];
        return long.Parse(numPart);
    }

    public static long ParseVmStat(string vmStat, string key)
    {
        var line = vmStat.Split('\n')
            .FirstOrDefault(l => l.Contains(key));

        if (line != null)
        {
            var parts = line.Split(':')[1].Trim().TrimEnd('.');
            return long.Parse(parts);
        }

        return 0;
    }
}