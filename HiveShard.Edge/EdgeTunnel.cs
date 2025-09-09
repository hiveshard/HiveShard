using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Fabric.Edge;
using HiveShard.Interface;

namespace HiveShard.Edge
{
    public class EdgeTunnel: IEdgeTunnel
    {
        private IEdgeTunnelServerEndpoint _edgeTunnel;
        private IAddressProvider _addressProvider;
        private ICancellationProvider _cancellationProvider;

        public EdgeTunnel(IEdgeTunnelServerEndpoint edgeTunnel, IAddressProvider addressProvider, ICancellationProvider cancellationProvider)
        {
            _edgeTunnel = edgeTunnel;
            _addressProvider = addressProvider;
            _cancellationProvider = cancellationProvider;
        }

        public void RegisterEdgeHandler<TEvent>(Action<TEvent, Client> handler)
        {
            _edgeTunnel.RegisterCallback((e,client) => handler((TEvent)e,client), typeof(TEvent));
        }
        public void SendEdgeEventToClient<TEvent>(TEvent @event, Client client)
        {
            _edgeTunnel.SendEvent(@event, typeof(TEvent), client);
        }

        public void SetClientConnectedCallback(Action<Client> handler)
        {
            _edgeTunnel.RegisterClientConnectedCallback(handler);
        }

        public async Task Start()
        {
            _ = _edgeTunnel.Start(_cancellationProvider.GetToken());
            await _edgeTunnel.WaitForReady();
        }
    }
}