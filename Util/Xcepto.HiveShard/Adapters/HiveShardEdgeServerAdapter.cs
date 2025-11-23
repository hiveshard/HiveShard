using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Edge;
using HiveShard.Interface;
using HiveShard.Provider;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.HiveShard.States;
using Xcepto.HiveShard.Util;

namespace Xcepto.HiveShard.Adapters
{
    public class HiveShardEdgeServerAdapter<T>: XceptoAdapter
    where T: BaseEdge
    {
        protected override Task Initialize(IServiceProvider serviceProvider) => Task.CompletedTask;

        protected override Task Cleanup(IServiceProvider serviceProvider)
        {
            var cancellationProvider = serviceProvider.GetRequiredService<CancellationProvider>();
            cancellationProvider.RequestCancellation();
            return Task.CompletedTask;
        }

        public void Action(Action<IEdgeTunnel> clientAction)
        {
            AddStep(new CompartmentalizedServiceBasedActionState<IEdgeTunnel>("Edge tunnel action", $"edge-{typeof(T).FullName}", x =>
            {
                clientAction(x);
                return Task.CompletedTask;
            })); 
        }

        public void Expect<T>(ClientExpectationPredicate<T> expectation)
            where T: IEvent
        {
            AddStep(new XceptoTcpServerExpectationState<T>($"Server Expectation of {typeof(T)}", expectation));
        }
    }
}