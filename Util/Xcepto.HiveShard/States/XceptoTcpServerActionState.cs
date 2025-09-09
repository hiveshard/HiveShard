using System;
using System.Threading.Tasks;
using HiveShard.Edge;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.States
{
    public class XceptoTcpServerActionState: XceptoState
    {
        private Action<IEdgeTunnel> _edgeServerAction;

        public XceptoTcpServerActionState(string name, Action<IEdgeTunnel> edgeServerAction) : base(name)
        {
            _edgeServerAction = edgeServerAction;
        }
    
        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider) 
            => Task.FromResult(true);

        public override Task OnEnter(IServiceProvider serviceProvider)
        {
            var magetownClient = serviceProvider.GetRequiredService<IEdgeTunnel>();
            _edgeServerAction(magetownClient);
            return Task.CompletedTask;
        }
    }
}