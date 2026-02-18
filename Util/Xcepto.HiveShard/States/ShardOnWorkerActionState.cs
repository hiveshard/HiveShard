using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Util;
using HiveShard.Workers.Shard.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.States;
using Xcepto.Util;

namespace Xcepto.HiveShard.States;

public class ShardOnWorkerActionState<TService>: XceptoState
{
    private readonly string _compartmentIdentifier;
    private readonly HiveShardIdentity _hiveShardIdentity;
    private readonly Action<TService> _action;
    private readonly string _name;

    public ShardOnWorkerActionState(string name, string compartmentIdentifier, HiveShardIdentity hiveShardIdentity, Action<TService> action) : base(name)
    {
        _name = name;
        _action = action;
        _hiveShardIdentity = hiveShardIdentity;
        _compartmentIdentifier = compartmentIdentifier;
    }

    public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider) =>
        Task.FromResult(true);

    public override Task OnEnter(IServiceProvider serviceProvider)
    {
        var cancellationProvider = serviceProvider.GetRequiredService<ICancellationProvider>();
        var loggingProvider = serviceProvider.GetRequiredService<IHiveShardTelemetry>();
        return Resilience.Retry(_ =>
        {
            var repository = serviceProvider.GetCompartmentalizedService<HiveShardRepository>(_compartmentIdentifier);
            if (!repository.TryGetHiveShard(_hiveShardIdentity, out IServiceProvider shardServiceProvider))
                throw new Exception($"HiveShard of type {_hiveShardIdentity.ShardType.GetShardType().Name} " +
                                    $"not found on {_compartmentIdentifier}");
            var requiredService = shardServiceProvider.GetRequiredService<TService>();
            _action(requiredService);
            return Task.CompletedTask;
        }, $"{nameof(ShardOnWorkerActionState<TService>)}: {_name}", cancellationProvider.GetToken(), loggingProvider);
    }
}