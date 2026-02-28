using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Builder;
using Xcepto.Data;
using Xcepto.HiveShard.Providers;
using Xcepto.Interfaces;
using Xcepto.Provider;
using Xcepto.Scenarios;

namespace Xcepto.HiveShard.Scenario;

public class HiveShardScenario: CompartmentalizedXceptoScenario
{
    private readonly ServiceEnvironment _environment;

    public HiveShardScenario(ServiceEnvironment environment)
    {
        _environment = environment;
    }

    protected override Task<IEnumerable<Compartment>> Setup()
    {
        Dictionary<string, Compartment> compartments = new();
            
            
        var sharedServices = _environment.Outer
            .AddSingleton<ServiceEnvironment>(_environment)
            .AddSingleton<ILoggingProvider, HiveShardTestTelemetryProvider>();
            
        var compartmentBuilder = Compartment.From(sharedServices);
        string outerIdentification = "outer";
        foreach (var serviceDescriptor in sharedServices)
        {
            compartmentBuilder.ExposeService(serviceDescriptor.ServiceType);
            compartmentBuilder.Identify(outerIdentification);
        }
        compartments.Add(outerIdentification, compartmentBuilder.Build());
            
            
        foreach (var compartmentEnvironment in _environment.Inner)
        {
            var innerServiceCollection = compartmentEnvironment.Services;
            GenericEntryPoint genericEntryPoint = new GenericEntryPoint();
            innerServiceCollection.AddSingleton<GenericEntryPoint>(genericEntryPoint);

            var innerCompartmentBuilder = Compartment.From(innerServiceCollection);
            innerCompartmentBuilder.Identify(compartmentEnvironment.Identifier);
            foreach (var dependency in compartmentEnvironment.Dependencies) innerCompartmentBuilder.DependsOn(dependency);

            innerCompartmentBuilder.SetEntryPoint(typeof(GenericEntryPoint));
            var compartment = innerCompartmentBuilder.Build();
            genericEntryPoint.UpdateStartMethod(() =>
            {
                var requiredService =
                    compartment.Services.GetRequiredService(compartmentEnvironment.EntryPointType);
                if (requiredService is not IIsolatedEntryPoint isolatedEntryPoint)
                    throw new Exception("Incorrectly registered entryPoint");
                return isolatedEntryPoint;
            });
            compartments.Add(compartmentEnvironment.Identifier, compartment);
        }
            
        return Task.FromResult(compartments.Values.AsEnumerable());
    }

    protected override ScenarioCleanup Cleanup(ScenarioCleanupBuilder builder) => builder
        .Do(x =>
        {
            var telemetry = x.GetRequiredService<IHiveShardTelemetry>();
            telemetry.Dispose();
        })
        .Build();
}