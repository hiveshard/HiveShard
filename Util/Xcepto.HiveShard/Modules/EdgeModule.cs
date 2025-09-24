using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using HiveShard.Data;
using HiveShard.Edge;
using HiveShard.Interface;
using Xcepto.HiveShard.States;

namespace Xcepto.HiveShard.Modules
{
    public class EdgeModule: XceptoModule
    {
        public EdgeModule()
        {
        }

        public void Action(Func<IEdgeTunnel, Task> clientAction)
        {
            AddStep(new ServiceBasedActionState<IEdgeTunnel>("Edge Action", clientAction)); 
        }

        public void Expect<T>(Predicate<T> expectation)
            where T: IEvent
        {
            AddStep(new XceptoClientExpectationState<T>($"Edge Expectation of {typeof(T)}", expectation));
        }
    }
}