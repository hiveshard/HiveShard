using System;
using System.Threading.Tasks;
using HiveShard.Config;
using HiveShard.Edge;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Provider;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Providers;
using Xcepto.HiveShard.States;
using Xcepto.HiveShard.Util;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard.Adapters
{
    public class EdgeServerAdapter: XceptoAdapter
    {
        protected override Task AddServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEdgeTunnel, EdgeTunnel>()
                .AddSingleton<ILoggingProvider, LoggingProvider>()
                .AddSingleton<IIdentityConfig>(new IdentityConfig(Guid.NewGuid(), "test"))
                .AddSingleton<IAddressProvider, EdgeIdentityProvider>();
            return Task.CompletedTask;
        }
    
        protected override Task Initialize(IServiceProvider serviceProvider) => Task.CompletedTask;

        protected override Task Cleanup(IServiceProvider serviceProvider)
        {
            var cancellationProvider = serviceProvider.GetRequiredService<CancellationProvider>();
            cancellationProvider.RequestCancellation();
            return Task.CompletedTask;
        }

        public void Action(Action<IEdgeTunnel> clientAction)
        {
            AddStep(new XceptoTcpServerActionState("Server Action", clientAction)); 
        }

        public void Expect<T>(ClientExpectationPredicate<T> expectation)
            where T: IEvent
        {
            AddStep(new XceptoTcpServerExpectationState<T>($"Server Expectation of {typeof(T)}", expectation));
        }
    }
}