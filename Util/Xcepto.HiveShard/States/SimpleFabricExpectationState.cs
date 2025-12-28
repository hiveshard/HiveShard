using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.States;

namespace Xcepto.HiveShard.States
{
    public class SimpleFabricExpectationState<T>: XceptoState
        where T: IEvent
    {
        private Predicate<T> _predicate;
        private string _topic;

        private ConcurrentQueue<T> _messages = new ConcurrentQueue<T>();

        public SimpleFabricExpectationState(string name, Predicate<T> predicate, string topic) : base(name)
        {
            _topic = topic;
            _predicate = predicate;
        }

        public override Task Initialize(IServiceProvider serviceProvider)
        {
            var fabric = serviceProvider.GetRequiredService<ISimpleFabric>();
            fabric.Register<T>(_topic, x =>
            {
                _messages.Enqueue(((Consumption<T>)x).Message);
            });
            return Task.CompletedTask;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
        {
            while (!_messages.IsEmpty)
            {
                if (!_messages.TryDequeue(out var t))
                    break;

                if (_predicate(t))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public override Task OnEnter(IServiceProvider serviceProvider) => Task.CompletedTask;
    }
}