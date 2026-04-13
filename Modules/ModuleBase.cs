using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using sharpfetch.Modules;

public abstract class ModuleBase : IModule
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public virtual string Description => string.Empty;
    public virtual int Order => 100;
    public virtual bool EnabledByDefault => true;

    public async Task<ModuleResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var value = await GetValueAsync(cancellationToken);
            stopwatch.Stop();

            return ModuleResult.CreateSuccess(
                Id,
                DisplayName,
                value,
                stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return ModuleResult.CreateError(
                Id,
                DisplayName,
                ex.Message,
                stopwatch.Elapsed
            );
        }
    }

    protected abstract Task<string> GetValueAsync(CancellationToken cancellationToken);

    protected Task<string> FromResult(string value) => Task.FromResult(value);

    protected async Task<string> RunAsync(Func<string> operation, CancellationToken cancellationToken)
    {
        return await Task.Run(operation, cancellationToken);
    }
}