using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Data;
using HiveShard.Interface;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Modules
{
    public class ClientModule: XceptoModule
    {
        private HiveShardClient _hiveShardClient;
        public ClientModule(HiveShardClient hiveShardClient)
        {
            _hiveShardClient = hiveShardClient;
        }

        public void Action(Func<IClientTunnel, Task> clientAction)
        {
            AddStep(new ServiceBasedActionState<IClientTunnel>("Client Action", clientAction)); 
        }

        public void Expect<T>(Predicate<T> expectation)
            where T: IEvent
        {
            AddStep(new XceptoClientExpectationState<T>($"Client Expectation of {typeof(T)}", expectation));
        }
    }
}