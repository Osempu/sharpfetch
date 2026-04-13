using System.Security.Cryptography;

namespace sharpfetch.Configuration;

public class SharpFetchConfiguration
{
    public ModuleConfiguration Modules { get; set; } = new();

    public DisplayConfiguration Display { get; set; } = new();
}

public class ModuleConfiguration
{
    public List<string> Modules { get; set; } = new();
    public bool ParallelExecution { get; set; } = true;
    public int TimeoutMs { get; set; } = 5000;
    public bool ShowExecutionTime { get; set; } = false;
}

public class DisplayConfiguration
{
    public string Format { get; set; } = "panel";
    public bool ShowIcons { get; set; } = true;
    public string ColorScheme { get; set; } = "default";
    public bool ShowCharts { get; set; } = true;
}