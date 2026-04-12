using System;
using System.Threading.Tasks;
using HiveShard.Client.Interfaces;
using HiveShard.Data;
using HiveShard.Interface;
using Xcepto.Adapters;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Adapters;

public class HiveShardClientAdapter: XceptoAdapter
{
    private readonly CompartmentIdentifier _compartmentIdentifier;

    public HiveShardClientAdapter(HiveShardClient client)
    {
        _compartmentIdentifier = new CompartmentIdentifier(client.UserId, CompartmentType.Client);
    }

    public void Action(Func<IClientTunnel, Task> clientAction)
    {
        AddStep(new CompartmentalizedServiceBasedActionState<IClientTunnel>("Client Action", _compartmentIdentifier, clientAction)); 
    }

    public void Expect<T>(Predicate<T> expectation)
        where T: IEvent
    {
        AddStep(new CompartmentalizedClientExpectationState<T>($"Client Expectation of {typeof(T)}", _compartmentIdentifier, expectation));
    }
}