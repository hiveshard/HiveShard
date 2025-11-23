using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Data;
using HiveShard.Interface;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardShardAdapter:XceptoAdapter
    {
        private string _compartmentIdentifier;

        public HiveShardShardAdapter(ShardType shardType, Chunk chunk)
        {
            _compartmentIdentifier = $"shard-{shardType.Name}-{chunk}";
        }
        
        public void Action(Func<IScopedShardTunnel, Task> clientAction)
        {
            AddStep(new CompartmentalizedServiceBasedActionState<IScopedShardTunnel>($"HiveShard {_compartmentIdentifier} action", _compartmentIdentifier, clientAction)); 
        }
        
        public void Expect<T>(Predicate<T> expectation)
            where T: IEvent
        {
            AddStep(new CompartmentalizedClientExpectationState<T>($"HiveShard {_compartmentIdentifier} expectation of type {typeof(T)}", _compartmentIdentifier, expectation));
        }
    }
}