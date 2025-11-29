using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
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
        AddStep(new ShardOnWorkerFabricExpectationState<TEvent>($"HiveShard {_compartmentIdentifier} expectation of type {typeof(TEvent)}", _compartmentIdentifier, _hiveShardIdentity, expectation));
    }
}