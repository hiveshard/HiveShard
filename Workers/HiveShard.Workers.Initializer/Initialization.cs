using System;
using System.Threading.Tasks;
using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;
using HiveShard.Interface.Repository;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initializer.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Initializer;

public class Initialization : IIsolatedEntryPoint
{
    private readonly InitializerAdditionRepository _initializers;
    private readonly GlobalChunkConfig _globalChunkConfig;
    private readonly ISimpleFabric _fabric;
    private readonly IEventRepository _eventRepository;
    public Initialization(InitializerAdditionRepository initializers, GlobalChunkConfig globalChunkConfig, ISimpleFabric fabric, IEventRepository eventRepository)
    {
        _initializers = initializers;
        _globalChunkConfig = globalChunkConfig;
        _fabric = fabric;
        _eventRepository = eventRepository;
    }

    public async Task Start()
    {
        while (_initializers.TryGetInitializer(out InitializerAdditionRequest request))
        {
            InitializationTunnel initializationTunnel = new InitializationTunnel(_fabric, _eventRepository);
            
            ServiceCollection initializerCollection = new ServiceCollection();
            initializerCollection.AddSingleton(typeof(IInitializer),request.Type);
            initializerCollection.AddSingleton(_globalChunkConfig);
            initializerCollection.AddSingleton<IInitializationTunnel>(initializationTunnel);

            var serviceProvider = initializerCollection.BuildServiceProvider();

            var initializerInstance = serviceProvider.GetRequiredService<IInitializer>();
            initializationTunnel.Initialize(initializerInstance, request.EmitterIdentity);
        }
    }
}