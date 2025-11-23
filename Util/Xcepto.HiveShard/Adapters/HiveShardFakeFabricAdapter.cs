using System;
using System.Threading.Tasks;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardFakeFabricAdapter: XceptoAdapter
    {
        public void FabricAction(Action<ISimpleFabric> action)
        {
            AddStep(new SimpleFabricActionState("Simple Fabric action", action));
        }

        public void FabricExpectation<T>(Predicate<T> predicate, string topic)
            where T: IEvent 
        {
            AddStep(new SimpleFabricExpectationState<T>("Simple fabric expectation state", predicate, topic));
        }
    }
}