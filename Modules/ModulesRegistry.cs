using sharpfetch.Configuration;

namespace sharpfetch.Modules;

public class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new();
    private static readonly Lazy<ModuleRegistry> _instance = new(() => new ModuleRegistry());

    public static ModuleRegistry Instance => _instance.Value;

    private ModulesRegistry()
    {
        DiscoverModules();
    }

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

    public void Register(IModule module)
    {
        _modules[module.Id] = module;
    }

    public IModule? GetModules(string id)
    {
        return _modules.TryGetValue(id, out var module) ? module : null;
    }

    public IEnumerable<IModule> GetAllModules(IEnumerable<string> ids)
    {
        return ids.Select(id => GetModules(id))
            .Where(m => m != null)
            .Cast<IModule>()
            .OrderBy(m => m.Order);
    }

    public IEnumerable<IModule> GetEnabledModules(ModuleConfiguration config)
    {
        if (config.Modules.Any())
        {
            return GetModules(config.Modules);
        }

        return GetAllModules().Where(m => m.EnabledByDefault);
    }

    public IEnumerable<string> GetModuleIds()
    {
        return _modules.Keys.OrderBy(k => k);
    }
}