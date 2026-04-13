using System.ComponentModel;

namespace sharpfetch.Modules;

public class ModuleExecutor
{
    private readonly ModuleRegistry _registry;

    public ModuleExecutor(ModuleRegistry? registry = null)
    {
        _registry = registry ?? ModuleRegistry.Instance;
    }

    public async Task<IReadOnlyList<ModuleResult>> ExecuteSequentialAsync(
        IEnumerable<IModule> modules,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ModuleResult>();

        foreach (var module in modules)
        {
            var result = await module.ExecuteAsync(cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<IReadOnlyList<ModuleResult>> ExecuteParallelAsync(
        IEnumerable<IModule> modules,
        CancellationToken cancellationToken = default)
    {
        var tasks = modules.Select(m => m.ExecuteAsync(cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results.OrderBy(r => modules.First(m => m.Id == r.ModuleId).Order).ToList();
    }

    public async Task<IReadOnlyList<ModuleResult>> ExecuteFromConfigAsync(
        ModuleConfiguration config,
        CancellationToken cancellationToken = default)
    {
        var modules = _registry.GetEnabledModules(config);

        return config.ParallelExecution
            ? await ExecuteParallelAsync(modules, cancellationToken)
            : await ExecuteSequentialAsync(modules, cancellationToken);
    }

}