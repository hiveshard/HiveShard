using System;
using System.Threading.Tasks;
using HiveShard.Fabric.Client;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.States
{
    public class SendEdgeMessageState<T>: XceptoState
    where T: IEvent
    {
        private T _message;

        public SendEdgeMessageState(string name, T message) : base(name)
        {
            _message = message;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider) =>
            Task.FromResult(true);

        public override Task OnEnter(IServiceProvider serviceProvider)
        {
            var clientEndpoint = serviceProvider.GetRequiredService<IEdgeTunnelClientEndpoint>();

            return clientEndpoint.SendEvent(_message, typeof(T));
        }
    }
}