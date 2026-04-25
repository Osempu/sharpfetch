using sharpfetch.Configuration;

namespace sharpfetch.Modules;

/// <summary>Central registry for all available modules.</summary>
public class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new();
    private static readonly Lazy<ModuleRegistry> _instance = new(() => new ModuleRegistry());

    public static ModuleRegistry Instance => _instance.Value;

    private ModuleRegistry()
    {
        DiscoverModules();
    }

    /// <summary>Discovers all IModule implementations in the current assembly via reflection.</summary>
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

    /// <summary>Manually register a module, replacing any existing registration for the same Id.</summary>
    public void Register(IModule module) => _modules[module.Id] = module;

    /// <summary>Get a specific module by Id, or null if not found.</summary>
    public IModule? GetModule(string id)
        => _modules.TryGetValue(id, out var module) ? module : null;

    /// <summary>Get all registered modules ordered by Order.</summary>
    public IEnumerable<IModule> GetAllModules()
        => _modules.Values.OrderBy(m => m.Order);

    /// <summary>Get modules by an explicit list of Ids, preserving Order.</summary>
    public IEnumerable<IModule> GetModules(IEnumerable<string> ids)
        => ids.Select(GetModule).Where(m => m != null).Cast<IModule>().OrderBy(m => m.Order);

    /// <summary>Get all modules belonging to a built-in group (case-insensitive).</summary>
    public IEnumerable<IModule> GetModulesByGroup(string group)
        => _modules.Values
            .Where(m => m.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.Order);

    /// <summary>Get all enabled modules based on configuration.</summary>
    public IEnumerable<IModule> GetEnabledModules(ModuleConfiguration config)
    {
        if (config.Modules.Count > 0)
            return GetModules(config.Modules);

        return GetAllModules().Where(m => m.EnabledByDefault);
    }

    /// <summary>
    /// Resolves the effective display groups for the given configuration.
    /// <para>
    /// When <see cref="ModuleConfiguration.Groups"/> is non-empty, those groups
    /// are used (each group collects modules by its explicit module list, or by
    /// matching the built-in Group value when the list is empty).
    /// </para>
    /// <para>
    /// When no groups are configured, modules are auto-grouped by their built-in
    /// <see cref="IModule.Group"/> value; the order of groups follows the first
    /// module encountered in each group.
    /// </para>
    /// </summary>
    public IReadOnlyList<ResolvedGroup> GetGroupedModules(ModuleConfiguration config)
    {
        var enabled = GetEnabledModules(config).ToList();

        if (config.Groups.Count > 0)
            return ResolveConfiguredGroups(config.Groups, enabled);

        return ResolveAutoGroups(enabled);
    }

    private static IReadOnlyList<ResolvedGroup> ResolveConfiguredGroups(
        IEnumerable<GroupConfiguration> groupConfigs,
        IReadOnlyList<IModule> enabledModules)
    {
        var groups = new List<ResolvedGroup>();
        var assignedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var gc in groupConfigs)
        {
            IEnumerable<IModule> members;

            if (gc.Modules.Count > 0)
            {
                // Explicit module list takes priority
                members = gc.Modules
                    .Select(id => enabledModules.FirstOrDefault(
                        m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    .Where(m => m != null)
                    .Cast<IModule>()
                    .OrderBy(m => m.Order);
            }
            else
            {
                // Auto-collect by matching the built-in Group value
                members = enabledModules
                    .Where(m => m.Group.Equals(gc.Id, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.Order);
            }

            var memberList = members.ToList();
            foreach (var m in memberList)
                assignedIds.Add(m.Id);

            if (memberList.Count > 0)
                groups.Add(new ResolvedGroup(gc.ResolvedDisplayName, gc.Color, memberList));
        }

        // Any enabled modules not assigned to any group go into an implicit "Other" bucket
        var unassigned = enabledModules
            .Where(m => !assignedIds.Contains(m.Id))
            .OrderBy(m => m.Order)
            .ToList();

        if (unassigned.Count > 0)
            groups.Add(new ResolvedGroup("Other", "grey", unassigned));

        return groups;
    }

    private static IReadOnlyList<ResolvedGroup> ResolveAutoGroups(IReadOnlyList<IModule> enabledModules)
    {
        // Preserve insertion order so groups appear in the order their first member appears
        var groupOrder = new List<string>();
        var groupMap = new Dictionary<string, List<IModule>>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in enabledModules)
        {
            if (!groupMap.ContainsKey(module.Group))
            {
                groupOrder.Add(module.Group);
                groupMap[module.Group] = [];
            }
            groupMap[module.Group].Add(module);
        }

        return groupOrder
            .Select(key => new ResolvedGroup(
                DisplayName: ToTitleCase(key) + " Info",
                Color: "green",
                Modules: groupMap[key]))
            .ToList();
    }

    /// <summary>Get all registered module Ids, sorted alphabetically.</summary>
    public IEnumerable<string> GetModuleIds() => _modules.Keys.OrderBy(k => k);

    private static string ToTitleCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpper(value[0]) + value[1..];
}

/// <summary>A resolved display group ready to be rendered.</summary>
public record ResolvedGroup(string DisplayName, string Color, IReadOnlyList<IModule> Modules);
