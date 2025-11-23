using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Interface;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardClientAdapter: XceptoAdapter
    {
        private string _username;

        public HiveShardClientAdapter(String username)
        {
            _username = username;
        }

        public void Action(Func<IClientTunnel, Task> clientAction)
        {
            AddStep(new CompartmentalizedServiceBasedActionState<IClientTunnel>("Client Action", _username, clientAction)); 
        }

        public void Expect<T>(Predicate<T> expectation)
            where T: IEvent
        {
            AddStep(new CompartmentalizedClientExpectationState<T>($"Client Expectation of {typeof(T)}", _username, expectation));
        }
    }
}