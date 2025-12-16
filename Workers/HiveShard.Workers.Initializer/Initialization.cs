using System;
using System.Threading.Tasks;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;
using HiveShard.Workers.Initializer.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Initializer;

public class Initialization : IIsolatedEntryPoint
{
    private InitializerAdditionRepository _initializers;
    private InitializationTunnel _initializationTunnel;
    public Initialization(InitializerAdditionRepository initializers, InitializationTunnel initializationTunnel)
    {
        _initializers = initializers;
        _initializationTunnel = initializationTunnel;
    }

    public async Task Start()
    {
        while (_initializers.TryGetInitializer(out Type initializer))
        {
            ServiceCollection initializerCollection = new ServiceCollection();
            initializerCollection.AddSingleton(typeof(IInitializer),initializer);

            var serviceProvider = initializerCollection.BuildServiceProvider();

            var initializerInstance = serviceProvider.GetRequiredService<IInitializer>();
            await initializerInstance.Initialize(_initializationTunnel);
        }

        await _initializationTunnel.FinalizeInitialization();
    }
}