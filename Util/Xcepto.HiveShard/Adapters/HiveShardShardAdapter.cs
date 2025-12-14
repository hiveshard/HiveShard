using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters;

public class HiveShardShardAdapter: XceptoAdapter
{
    private HiveShardIdentity _hiveShardIdentity;
    private string _compartmentIdentifier;
    public HiveShardShardAdapter(string worker, HiveShardIdentity hiveShardIdentity)
    {
        _compartmentIdentifier = $"shardWorker-{worker}";
        _hiveShardIdentity = hiveShardIdentity;
    }
    
    public void Action<TService>(Action<TService> clientAction)
    {
        AddStep(new ShardOnWorkerActionState<TService>($"HiveShard {_compartmentIdentifier} action", _compartmentIdentifier, _hiveShardIdentity, clientAction)); 
    }
        
    public void ExpectEvent<TEvent>(Predicate<TEvent> expectation)
        where TEvent: IEvent
    {
        AddStep(new ShardOnWorkerFabricExpectationState<TEvent>($"HiveShard {_compartmentIdentifier} event expectation of type {typeof(TEvent)}", _compartmentIdentifier, _hiveShardIdentity, expectation));
    }

    public void Except<TService>(Predicate<TService> expectation)
    where TService: notnull
    {
        AddStep(new ShardOnWorkerServiceExpectationState<TService>($"HiveShard {_compartmentIdentifier} service expectation of type {typeof(TService)}", _compartmentIdentifier, _hiveShardIdentity, expectation));
    }
}