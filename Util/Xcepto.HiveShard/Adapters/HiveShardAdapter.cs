using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiveShard.Client;
using HiveShard.Client.Interface;
using HiveShard.Data;
using HiveShard.Fabric.Client;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Modules;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardAdapter: XceptoAdapter
    {
        private IServiceProvider _serviceProvider;
        private ServiceEnvironment _environment;

        protected override Task Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _environment = _serviceProvider.GetRequiredService<ServiceEnvironment>();
            return Task.CompletedTask;
        }

        public void SendEdgeMessage<T>(T message)
        where T: IEvent
        {
            AddStep(new SendEdgeMessageState<T>($"Send message of type {typeof(T).Name}", message));
        }
        
        public void ExpectEdgeMessage<T>(Predicate<T> predicate)
            where T: IEvent
        {
            throw new System.NotImplementedException();
        }

        public void SendShardMessage<T>(T message)
        where T: IEvent
        {
            throw new System.NotImplementedException();
        }
        
        public void ExpectShardMessage<T>(Predicate<T> predicate)
            where T: IEvent
        {
            throw new System.NotImplementedException();
        }

        public ClientModule CreateClientModule()
        {
            var edgeEndpoint = _serviceProvider.GetRequiredService<IEdgeTunnelClientEndpoint>();
            var client = new HiveShardClient($"test-client-{Guid.NewGuid()}");
            var cancellationProvider = _serviceProvider.GetRequiredService<ICancellationProvider>();
            var clientTunnel = new ClientTunnel(edgeEndpoint, client, cancellationProvider);

            return new ClientModule(client);
        }

        public object CreateEdgeModule()
        {
            throw new NotImplementedException();
        }
    }
}