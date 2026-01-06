using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.States;

namespace Xcepto.HiveShard.States;

public class TickerTickExpectationState: XceptoState
{
    public TickerTickExpectationState(string name, Partition partition, Predicate<Tick> predicate) : base(name)
    {
        _partition = partition;
        _predicate = predicate;
    }

    
    private Partition _partition;
    private Predicate<Tick> _predicate;

    private ConcurrentQueue<Tick> _messages = new ConcurrentQueue<Tick>();
    public override Task Initialize(IServiceProvider serviceProvider)
    {
        var fabric = serviceProvider.GetRequiredService<ISimpleFabric>();
        fabric.Register<Tick>("ticks", _partition, x =>
        {
            _messages.Enqueue(((Consumption<Tick>)x).Message);
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