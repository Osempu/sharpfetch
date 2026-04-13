using sharpfetch.Configuration;

namespace sharpfetch.Modules;

/// <summary>
/// Central registry for all available modules.
/// </summary>
public class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new();
    private static readonly Lazy<ModuleRegistry> _instance = new(() => new ModuleRegistry());

    public static ModuleRegistry Instance => _instance.Value;

    private ModuleRegistry()
    {
        // Auto-discover and register all modules
        DiscoverModules();
    }

    /// <summary>
    /// Discovers all IModule implementations via reflection.
    /// </summary>
    private void DiscoverModules()
    {
        var moduleType = typeof(IModule);
        var modules = typeof(ModuleRegistry).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && moduleType.IsAssignableFrom(t))
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .ToList();

        foreach (var module in modules)
        {
            Register(module);
        }
    }

    /// <summary>
    /// Manually register a module.
    /// </summary>
    public void Register(IModule module)
    {
        _modules[module.Id] = module;
    }

    /// <summary>
    /// Get a specific module by ID.
    /// </summary>
    public IModule? GetModule(string id)
    {
        return _modules.TryGetValue(id, out var module) ? module : null;
    }

    /// <summary>
    /// Get all registered modules.
    /// </summary>
    public IEnumerable<IModule> GetAllModules()
    {
        return _modules.Values.OrderBy(m => m.Order);
    }

    /// <summary>
    /// Get modules filtered by IDs.
    /// </summary>
    public IEnumerable<IModule> GetModules(IEnumerable<string> ids)
    {
        return ids.Select(id => GetModule(id))
            .Where(m => m != null)
            .Cast<IModule>()
            .OrderBy(m => m.Order);
    }

    /// <summary>
    /// Get all enabled modules based on configuration.
    /// </summary>
    public IEnumerable<IModule> GetEnabledModules(ModuleConfiguration config)
    {
        if (config.Modules.Any())
        {
            // If specific modules are listed, use only those
            return GetModules(config.Modules);
        }

        // Otherwise, return all modules enabled by default
        return GetAllModules().Where(m => m.EnabledByDefault);
    }

    /// <summary>
    /// Get available module IDs.
    /// </summary>
    public IEnumerable<string> GetModuleIds()
    {
        return _modules.Keys.OrderBy(k => k);
    }
}