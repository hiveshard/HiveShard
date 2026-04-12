using System;
using System.Threading.Tasks;
using HiveShard.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.States;

namespace Xcepto.HiveShard.States;

public class SimpleFabricActionState: XceptoState
{
    private readonly Action<ISimpleFabric> _action;

    public SimpleFabricActionState(string name, Action<ISimpleFabric> action) : base(name)
    {
        _action = action;
    }

    public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider) 
        => Task.FromResult(true);

    public override Task OnEnter(IServiceProvider serviceProvider)
    {
        var requiredService = serviceProvider.GetRequiredService<ISimpleFabric>();
        _action(requiredService);
        return Task.CompletedTask;
    }
}