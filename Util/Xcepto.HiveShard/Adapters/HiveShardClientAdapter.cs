using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Interface;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardClientAdapter: XceptoAdapter
    {
        private string _compartmentIdentifier;

        public HiveShardClientAdapter(String username)
        {
            _compartmentIdentifier = $"client-{username}";
        }

        public void Action(Func<IClientTunnel, Task> clientAction)
        {
            AddStep(new CompartmentalizedServiceBasedActionState<IClientTunnel>("Client Action", _compartmentIdentifier, clientAction)); 
        }

        public void Expect<T>(Predicate<T> expectation)
            where T: IEvent
        {
            AddStep(new CompartmentalizedClientExpectationState<T>($"Client Expectation of {typeof(T)}", _compartmentIdentifier, expectation));
        }
    }
}