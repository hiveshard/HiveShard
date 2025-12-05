using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Util;
using HiveShard.Workers.Shard.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.States;
using Xcepto.Util;

namespace Xcepto.HiveShard.States;

public class ShardOnWorkerFabricExpectationState<TEvent>: XceptoState
where TEvent: IEvent
{
    private string _compartmentIdentifier;
    private HiveShardIdentity _hiveShardIdentity;
    private Predicate<TEvent> _expectation;

    private ConcurrentQueue<TEvent> _events = new();
    private string _name;

    public ShardOnWorkerFabricExpectationState(string name, string compartmentIdentifier, HiveShardIdentity hiveShardIdentity, Predicate<TEvent> expectation) : base(name)
    {
        _name = name;
        _expectation = expectation;
        _hiveShardIdentity = hiveShardIdentity;
        _compartmentIdentifier = compartmentIdentifier;
    }

    public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
    {
        while (_events.TryDequeue(out TEvent e))
        {
            if (_expectation(e))
                return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public override Task OnEnter(IServiceProvider serviceProvider)
    {
        var cancellationProvider = serviceProvider.GetRequiredService<ICancellationProvider>();
        var debugLoggingProvider = serviceProvider.GetRequiredService<IDebugLoggingProvider>();
        return Resilience.Retry(_ =>
        {
            var hiveShardRepository =
                serviceProvider.GetCompartmentalizedService<HiveShardRepository>(_compartmentIdentifier);
            if (!hiveShardRepository.TryGetHiveShard(_hiveShardIdentity, out var provider))
                throw new Exception($"HiveShard {_hiveShardIdentity.ShardType.GetShardType().Name} " +
                                    $"not found on {_compartmentIdentifier}");
            var scopedShardTunnel = provider.GetRequiredService<IScopedShardTunnel>();
            scopedShardTunnel.Register<TEvent>(e => _events.Enqueue(e));
            return Task.CompletedTask;
        }, $"{nameof(ShardOnWorkerFabricExpectationState<TEvent>)}: {_name}", cancellationProvider.GetToken(), debugLoggingProvider);
    }
}