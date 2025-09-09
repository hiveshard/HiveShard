using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HiveShard.Edge;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.Util;

namespace Xcepto.HiveShard.States
{
    public class XceptoTcpServerExpectationState<T>: XceptoState
    {
        private ClientExpectationPredicate<T> _clientExpectationPredicate;
        private ConcurrentQueue<(T, global::HiveShard.Data.Client)> _messages = new();
    
        public XceptoTcpServerExpectationState(string name, ClientExpectationPredicate<T> clientExpectationPredicate) : base(name)
        {
            _clientExpectationPredicate = clientExpectationPredicate;
        }

        public override Task Initialize(IServiceProvider serviceProvider)
        {
            var edgeTunnel = serviceProvider.GetRequiredService<IEdgeTunnel>();
            edgeTunnel.RegisterEdgeHandler<T>((newValue, client) =>
            {
                _messages.Enqueue((newValue, client));
            });
            return Task.CompletedTask;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
        {
            while (!_messages.IsEmpty)
            {
                if (_messages.TryDequeue(out var message))
                {
                    var payload = message.Item1;
                    var client = message.Item2;

                    if (_clientExpectationPredicate(payload, client))
                        return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public override Task OnEnter(IServiceProvider serviceProvider) => Task.CompletedTask;
    }
}