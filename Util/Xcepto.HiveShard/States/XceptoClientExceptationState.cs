using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.States
{
    public class XceptoClientExpectationState<T>: XceptoState
    {
        private Predicate<T> _clientPredicate;
        private ConcurrentQueue<T?> _messages = new();
    
        public XceptoClientExpectationState(string name, Predicate<T> clientPredicate) : base(name)
        {
            _clientPredicate = clientPredicate;
        
        }

        public override Task Initialize(IServiceProvider serviceProvider)
        {
            var clientTunnel = serviceProvider.GetRequiredService<IClientTunnel>();
            clientTunnel.RegisterHotPathEventHandler<T>(newValue =>
            {
                _messages.Enqueue(newValue);
            });
            return Task.CompletedTask;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
        {
            while (!_messages.IsEmpty)
            {
                if (_messages.TryDequeue(out var t))
                {
                    if(t is null)
                        continue;

                    if (_clientPredicate(t))
                        return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public override Task OnEnter(IServiceProvider serviceProvider) => Task.CompletedTask;
    }
}