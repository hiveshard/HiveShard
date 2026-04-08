using HiveShard.Data;
using HiveShard.Event;
using HiveShard.Fabrics.InMemory;
using HiveShard.Initializer.Interfaces;
using HiveShard.Interface;
using HiveShard.Interface.Logging;
using HiveShard.Repository;
using HiveShard.Serializer;
using HiveShard.Telemetry.Console;
using HiveShard.Util.Test;
using HiveShard.Workers.Initialization.Tests.Data;
using HiveShard.Workers.Initializer;
using HiveShard.Workers.Initializer.Data;
using HiveShard.Workers.Initialization.Tests.Events;
using Microsoft.Extensions.DependencyInjection;

namespace HiveShard.Workers.Initialization.Tests.Util
{
    public class InitializationTunnelTest<TInitializer>
        where TInitializer : class, IInitializer
    {
        private readonly EventRepository _eventRepository;
        private readonly ISimpleFabric _fabric;
        private readonly TInitializer _initializer;
        private GlobalChunkConfig _globalChunkConfig;

        private static readonly Chunk Chunk = new(0, 0);

        public TInitializer Initializer => _initializer;

        private InitializationTunnelTest(
            EventRepository eventRepository,
            InitializerEmitterIdentity identity)
        {
            _eventRepository = eventRepository;

            _globalChunkConfig = new GlobalChunkConfig(Chunk, Chunk);
            _fabric = CreateFabric(_globalChunkConfig);

            var tunnel = new InitializationTunnel(_fabric, _eventRepository, _globalChunkConfig);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<GlobalChunkConfig>(_globalChunkConfig);
            serviceCollection.AddSingleton<TInitializer>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _initializer = serviceProvider.GetRequiredService<TInitializer>();

            tunnel.Initialize(_initializer, identity);
        }

        private ISimpleFabric CreateFabric(GlobalChunkConfig chunkConfig)
        {
            IHiveShardTelemetry telemetry = new SimpleConsoleTelemetry();
            ISerializer serializer = new NewtonsoftSerializer();
            return new InMemorySimpleFabric(telemetry, chunkConfig, serializer);
        }

        private Tick CreateTick<T>(long tick)
            where T : IEvent
        {
            return new Tick(
                tick,
                [],
                DateTime.UtcNow,
                typeof(T).FullName!,
                new TestEmitter().Identity
            );
        }

        public void SendTick<T>(int tick)
            where T : IEvent
        {
            var partition = new Partition(_eventRepository.GetEventOrder<T>());

            _fabric.Send(
                typeof(Tick).FullName!,
                partition,
                CreateTick<T>(tick)
            );
        }

        public IEnumerable<Consumption<IEnvelope<object>>> FetchCompletedTopic<T>(
            int from,
            int toExclusive)
            where T : IEvent
        {
            var partition = new Partition(_eventRepository.GetEventOrder<T>());

            return _fabric.FetchTopic(
                new TopicPartition(typeof(CompletedTick).FullName!, partition),
                from,
                toExclusive
            );
        }
        
        public IEnumerable<Consumption<IEnvelope<object>>> FetchTopic<T>(int from, int toExclusive)
        where T : IEvent
        {
            var topicPartition = new TopicPartition(typeof(T).FullName!, Chunk.ToPartition(_globalChunkConfig));
            return _fabric.FetchTopic(
                topicPartition,
                from,
                toExclusive
            );
        }

        public static InitializationTunnelTest<TInitializer> CreateTestInitializer()
        {
            var eventRepository = new EventRepository();

            var identity = new InitializerEmitterIdentity(new EmitterIdentity("initializer"));

            eventRepository.RegisterEvent<InitialDataEvent>(identity);

            return new InitializationTunnelTest<TInitializer>(eventRepository, identity);
        }
    }
}