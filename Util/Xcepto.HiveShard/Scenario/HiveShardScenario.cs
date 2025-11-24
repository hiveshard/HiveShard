using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Data;
using Xcepto.HiveShard.Util;
using Xcepto.Interfaces;
using Xcepto.Provider;
using Xcepto.Scenarios;

namespace Xcepto.HiveShard.Scenario
{
    public class HiveShardScenario: CompartmentalizedXceptoScenario
    {
        private ServiceEnvironment _environment;

        public HiveShardScenario(ServiceEnvironment environment)
        {
            _environment = environment;
        }

        protected override Task<IEnumerable<Compartment>> Setup()
        {
            List<Compartment> compartments = new List<Compartment>();
            
            
            var sharedServices = _environment.Outer
                .AddSingleton<ServiceEnvironment>(_environment)
                .AddSingleton<ILoggingProvider, XceptoBasicLoggingProvider>();
            
            var compartmentBuilder = Compartment.From(sharedServices);
            foreach (var serviceDescriptor in sharedServices)
            {
                compartmentBuilder.ExposeService(serviceDescriptor.ServiceType);
                compartmentBuilder.Identify("outer");
            }
            compartments.Add(compartmentBuilder.Build());
            
            
            foreach (var compartmentEnvironment in _environment.Inner)
            {
                var innerCompartmentBuilder = Compartment.From(compartmentEnvironment.Services);
                innerCompartmentBuilder.Identify(compartmentEnvironment.Identifier);
                foreach (var dependency in compartmentEnvironment.Dependencies)
                {
                    innerCompartmentBuilder.DependsOn(dependency);
                }
                compartments.Add(innerCompartmentBuilder.Build());
            }
            
            return Task.FromResult(compartments.AsEnumerable());
        }
    }
}