using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.States
{
    public class XceptoClientActionState: XceptoState
    {
        private Func<IClientTunnel, Task> _clientAction;

        public XceptoClientActionState(string name, Func<IClientTunnel, Task> clientAction) : base(name)
        {
            _clientAction = clientAction;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
            => Task.FromResult(true);

        public override async Task OnEnter(IServiceProvider serviceProvider)
        {
            var magetownClient = serviceProvider.GetRequiredService<IClientTunnel>();
            await _clientAction(magetownClient);
        }
    }
}