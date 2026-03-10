using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.States;

namespace Xcepto.HiveShard.States;

public class TickerExpectationState<TTickEvent>: XceptoState
where TTickEvent : ITickEvent
{
    public TickerExpectationState(string name, string topic, Partition partition, Predicate<TTickEvent> predicate) : base(name)
    {
        _topic = topic;
        _partition = partition;
        _predicate = predicate;
    }

    
    private readonly Partition _partition;
    private readonly Predicate<TTickEvent> _predicate;

    private readonly ConcurrentQueue<IEnvelope<TTickEvent>> _messages = new();
    private readonly string _topic;

    public override Task Initialize(IServiceProvider serviceProvider)
    {
        var fabric = serviceProvider.GetRequiredService<ISimpleFabric>();
        fabric.Register<TTickEvent>(_topic, _partition, x =>
        {
            _messages.Enqueue(((Consumption<IEnvelope<TTickEvent>>)x).Message);
        });
        return Task.CompletedTask;
    }

    public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
    {
        while (!_messages.IsEmpty)
        {
            if (!_messages.TryDequeue(out var t))
                break;

            if (_predicate(t.Payload)) return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override Task OnEnter(IServiceProvider serviceProvider) => Task.CompletedTask;
}