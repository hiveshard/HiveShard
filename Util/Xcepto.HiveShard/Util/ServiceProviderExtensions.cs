using System;
using Microsoft.Extensions.DependencyInjection;
using Xcepto.Repositories;

namespace Xcepto.HiveShard.Util
{
    public static class ServiceProviderExtensions
    {
        public static T GetCompartmentalizedService<T>(this IServiceProvider serviceProvider, string compartmentIdentifier)
        {
            var compartmentRepository = serviceProvider.GetRequiredService<CompartmentRepository>();
            var compartment = compartmentRepository.GetCompartment(compartmentIdentifier);
            return compartment.Services.GetRequiredService<T>();
        }
    }
}