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

public class ShardOnWorkerServiceExpectationState<TService>: XceptoState
where TService: notnull
{
    private string _compartmentIdentifier;
    private HiveShardIdentity _hiveShardIdentity;
    private Predicate<TService> _expectation;
    private TService? _service;
    private string _name;


    public ShardOnWorkerServiceExpectationState(string name, string compartmentIdentifier, HiveShardIdentity hiveShardIdentity, Predicate<TService> expectation) : base(name)
    {
        _name = name;
        _expectation = expectation;
        _hiveShardIdentity = hiveShardIdentity;
        _compartmentIdentifier = compartmentIdentifier;
    }

    public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
    {
        if (_service is null)
            throw new Exception($"Service {typeof(TService)} not found on {_compartmentIdentifier}");
        return Task.FromResult(_expectation(_service));
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
                _service = provider.GetRequiredService<TService>();
                return Task.CompletedTask;
            }, $"{nameof(ShardOnWorkerServiceExpectationState<TService>)}: {_name}", cancellationProvider.GetToken(), debugLoggingProvider);
    }
}