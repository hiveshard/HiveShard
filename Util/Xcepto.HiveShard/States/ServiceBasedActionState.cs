using System;
using System.Threading.Tasks;
using HiveShard.Client.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Xcepto.HiveShard.States
{
    public class ServiceBasedActionState<T>: XceptoState
    where T: class
    {
        private readonly Func<T, Task> _action;

        public ServiceBasedActionState(string name, Func<T, Task> action) : base(name)
        {
            this._action = action;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
            => Task.FromResult(true);

        public override async Task OnEnter(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<T>();
            await _action(service);
        }
    }
}