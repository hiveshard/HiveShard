using System;
using System.Threading.Tasks;
using HiveShard.Client;
using HiveShard.Client.Interface;
using HiveShard.Fabric.Client;
using HiveShard.Interface;
using HiveShard.Provider;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Providers;
using Xcepto.HiveShard.States;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard.Adapters
{
    public class EdgeClientAdapter: XceptoAdapter
    {
        private global::HiveShard.Data.Client _client;
        public EdgeClientAdapter(global::HiveShard.Data.Client client)
        {
            _client = client;
        }

        protected override Task Initialize(IServiceProvider serviceProvider) => Task.CompletedTask;

        protected override Task Cleanup(IServiceProvider serviceProvider)
        {
            var cancellationProvider = serviceProvider.GetRequiredService<CancellationProvider>();
            cancellationProvider.RequestCancellation();
            return Task.CompletedTask;
        }

        public void Action(Func<IClientTunnel, Task> clientAction)
        {
            AddStep(new XceptoClientActionState("Client Action", clientAction)); 
        }

        public void Expect<T>(Predicate<T> expectation)
            where T: IEvent
        {
            AddStep(new XceptoClientExpectationState<T>($"Client Expectation of {typeof(T)}", expectation));
        }
    }
}