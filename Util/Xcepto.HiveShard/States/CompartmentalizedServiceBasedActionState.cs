using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Repositories;
using Xcepto.States;

namespace Xcepto.HiveShard.States
{
    public class CompartmentalizedServiceBasedActionState<T>: XceptoState
    where T: class
    {
        private readonly Func<T, Task> _action;
        private string _compartmentIdentifier;

        public CompartmentalizedServiceBasedActionState(string name, 
            string compartmentIdentifier, Func<T, Task> action) : base(name)
        {
            _compartmentIdentifier = compartmentIdentifier;
            this._action = action;
        }

        public override Task<bool> EvaluateConditionsForTransition(IServiceProvider serviceProvider)
            => Task.FromResult(true);

        public override async Task OnEnter(IServiceProvider serviceProvider)
        {
            var compartmentRepository = serviceProvider.GetRequiredService<CompartmentRepository>();
            var compartment = compartmentRepository.GetCompartment(_compartmentIdentifier);
            var service = compartment.Services.GetRequiredService<T>();
            await _action(service);
        }
    }
}