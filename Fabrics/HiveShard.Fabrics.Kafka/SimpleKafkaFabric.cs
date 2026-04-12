using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Interface.Providers;
using HiveShard.Util;
using Partition = Confluent.Kafka.Partition;
using TopicPartition = HiveShard.Data.TopicPartition;

namespace HiveShard.Fabrics.Kafka
{
    public class SimpleKafkaFabric: ISimpleFabric
    {
        private readonly CancellationToken _ctsToken;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ProducerConfig _producerConfig;
        private readonly string _broker;
        private readonly ISerializer _serializer;
        private readonly IHiveShardTelemetry _scopedLogger;
        private readonly IEnvironmentConfig _environmentConfig;
        private readonly BlockingCollection<TopicChunk> _ensureTopics = new();
        private readonly ConcurrentQueue<KafkaRegistration> _kafkaRegistrations = new();
        private readonly ConcurrentQueue<TopicChunk> _newProducers = new();
        private readonly ConcurrentDictionary<TopicChunk, BlockingCollection<string>> _messagesToBeProduced = new();
        private readonly GlobalChunkConfig _globalChunkConfig;
        public SimpleKafkaFabric(IIdentityConfig identityConfig, ICancellationProvider cancellationProvider, ISerializer serializer, IHiveShardTelemetry fabricLoggingProvider, IEnvironmentConfig environmentConfig, GlobalChunkConfig globalChunkConfig)
        {
            _serializer = serializer;
            _environmentConfig = environmentConfig;
            _globalChunkConfig = globalChunkConfig;
            _ctsToken = cancellationProvider.GetToken();
            _broker = Environment.GetEnvironmentVariable("KAFKA_ENDPOINT") 
                      ?? "localhost:9092";

            _scopedLogger = fabricLoggingProvider.GetScopedLogger<SimpleKafkaFabric>(identityConfig);


            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _broker,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                GroupId = identityConfig.GetIdentityString(),
            };
            
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = _broker,
            };
        }

        public void CompleteDeliveries(int deliveries) => throw new NotImplementedException();

        public void Register<T>(string topic, EmitterIdentity consumer, Action<Consumption<IEnvelope<T>>> action) where T: IEvent
            => Register(topic, new Chunk(0, 0), consumer, action);

        public void Register<T>(string topic, Chunk chunk, EmitterIdentity consumer, Action<Consumption<IEnvelope<T>>> action) where T: IEvent
        {
            _ensureTopics.Add(new TopicChunk(topic, chunk), _ctsToken);
            _kafkaRegistrations.Enqueue(new KafkaRegistration(new TopicChunk(topic, chunk), (json, offset) =>
            {
                var message = _serializer.Deserialize<IEnvelope<T>>(json);
                action(new Consumption<IEnvelope<T>>(message, offset));
            }));
        }

        public void Register<T>(string topic, Data.Partition partition, EmitterIdentity consumer, Action<Consumption<IEnvelope<T>>> action)
            where T: IEvent => Register(topic, partition.ToChunk(_globalChunkConfig), consumer, action);

        public void Send<T>(string topic, IEnvelope<T> message) where T: IEvent
            => Send(topic, new Chunk(0,0), message);
        public void Send<T>(string topic, Chunk chunk, IEnvelope<T> message) where T: IEvent
        {
            TopicChunk topicChunk = new TopicChunk(topic, chunk);
            _messagesToBeProduced.AddOrUpdate(topicChunk,
                key =>
                {
                    _newProducers.Enqueue(topicChunk);
                    _ensureTopics.Add(topicChunk, _ctsToken);
                    return new BlockingCollection<string>();
                },
                (key, oldValue) => oldValue is null ? new BlockingCollection<string>() : oldValue);
            _messagesToBeProduced[topicChunk].Add(_serializer.Serialize(message));
            throw new NotImplementedException("no immediate access to offset");
        }

        public void Send<T>(string topic, Data.Partition partition, IEnvelope<T> message) where T: IEvent =>
            Send(topic, partition.ToChunk(_globalChunkConfig), message);

        public void Send<T>(string topic, Data.Partition partition, Func<BatchedOffsetResults, IEnvelope<T>> messageBuilder) where T : IEvent
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicChunk topicChunk, long fromOffset, long toOffsetExclusive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Consumption<IEnvelope<object>>> FetchTopic(TopicPartition topicPartition, long fromOffset, long toOffsetExclusive)
        {
            throw new NotImplementedException();
        }

        public Task Start()
        {
            throw new NotImplementedException();
            // CancellationToken token = new CancellationToken();
            // return Task.Run(async () =>
            // {
            //     List<Task> tasks = new List<Task>();
            //     tasks.Add(CreateAdminClient());
            //     while (!token.IsCancellationRequested)
            //     {
            //         if (_kafkaRegistrations.TryDequeue(out var kafkaRegistration)) tasks.Add(CreateConsumer(kafkaRegistration));
            //
            //         if (_newProducers.TryDequeue(out var newProducer)) tasks.Add(CreateProducer(newProducer, cancellationToken));
            //         await Task.Delay(100);
            //     }
            //     
            //     // after cancellation, propagate
            //     return Task.WhenAll(tasks);
            // }, token);
        }

        private Task CreateAdminClient()
        {
            return Task.Run(async () =>
            {
                IAdminClient adminClient = null;
                await Resilience.Retry(async token =>
                    {
                        if (adminClient is null)
                            adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _broker })
                                .SetLogHandler((_, logMessage) =>
                                {
                                    _scopedLogger.LogDebug($"{logMessage.Name}: {logMessage.Message}");
                                    if (logMessage.Level <= SyslogLevel.Error)
                                        throw new Exception($"{logMessage.Level}: {logMessage.Name}, {logMessage.Message}");
                                })
                                .Build();
                    }, "Create AdminClient", _ctsToken, _scopedLogger);
                
                while (true)
                {
                    var topicPartition = _ensureTopics.Take();
                    var topic = topicPartition.Prefixed(_environmentConfig);
                    int count =
                        (_globalChunkConfig.MaxChunk.XCoord - _globalChunkConfig.MinChunk.XCoord + 1) *
                        (_globalChunkConfig.MaxChunk.YCoord - _globalChunkConfig.MinChunk.YCoord + 1);
                    await adminClient.CreateTopicsAsync(new TopicSpecification[]
                    {
                        new() { Name = topic, NumPartitions = count, ReplicationFactor = 1 }
                    });
                    _scopedLogger.LogDebug($"Created topic {topic} with partitions: 1-{count}");
                }
            });
        }

        private Task CreateProducer(TopicChunk topicChunk, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                IProducer<Null, string> producer = null;
                await Resilience.Retry(async token =>
                {
                    if (producer is null)
                        producer = new ProducerBuilder<Null, string>(_producerConfig)
                            .SetLogHandler((_, logMessage) =>
                            {
                                if (logMessage.Level <= SyslogLevel.Error)
                                    _scopedLogger.LogException(new Exception($"{logMessage.Level}: {logMessage.Name}, {logMessage.Message}"));
                                else
                                    _scopedLogger.LogDebug($"{logMessage.Name}: {logMessage.Message}");
                            })
                            .Build();
                }, $"Publish {topicChunk.Topic}", _ctsToken, _scopedLogger);
                
                
                var blockingCollection = _messagesToBeProduced[topicChunk];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = blockingCollection.Take();
                    var delivery = await producer.ProduceAsync(new Confluent.Kafka.TopicPartition($"{_environmentConfig.Prefix}-{topicChunk.Topic}", new Partition(topicChunk.Chunk.ToPartition(_globalChunkConfig).Value)), 
                        new Message<Null, string> { Value = message, Headers = new Headers()
                        {
                            new Header("message-type", Encoding.UTF8.GetBytes(topicChunk.Topic))
                        }}, cancellationToken);
                    _scopedLogger.LogDebug($"Delivered to: {delivery.TopicPartitionOffset}");
                }
            }, cancellationToken);
        }
        
        private Task CreateConsumer(KafkaRegistration kafkaRegistration)
        {
            return Task.Run(async () =>
            {
                IConsumer<Ignore, string> consumer = null;
                await Resilience.Retry(_ =>
                {
                    consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig)
                        .SetLogHandler((_, logMessage) =>
                        {
                            if (logMessage.Level <= SyslogLevel.Error)
                                _scopedLogger.LogException(new Exception($"{logMessage.Level}: {logMessage.Name}, {logMessage.Message}"));
                            else
                                _scopedLogger.LogDebug($"{logMessage.Name}: {logMessage.Message}");
                        })
                        .Build();
                    if (consumer is null)
                        throw new Exception();
                    var partition = new Partition(kafkaRegistration.TopicChunk.Chunk.ToPartition(_globalChunkConfig).Value);
                    var kafkaTopicPartition = new Confluent.Kafka.TopicPartition(kafkaRegistration.TopicChunk.Prefixed(_environmentConfig), partition);
                    consumer.Assign(kafkaTopicPartition);
                    return Task.CompletedTask;
                    
                }, "Subscribe", _ctsToken, _scopedLogger);
                
                while (!_ctsToken.IsCancellationRequested)
                    try
                    {
                        var result = consumer!.Consume(_ctsToken);
                        kafkaRegistration.Action(result.Message.Value, result.Offset);
                    }
                    catch (ConsumeException consumeException)
                    {
                        _scopedLogger.LogWarning(consumeException.Message);
                    }
                    catch (Exception e)
                    {
                        var exception = new Exception($"💥 Unexpected error: {e}");
                        _scopedLogger.LogException(exception);
                        throw exception;
                    }

                consumer?.Close();
            }, _ctsToken);
        }
        
        public Task WaitForReady() => Task.CompletedTask;
    }

    public class KafkaRegistration
    {
        public TopicChunk TopicChunk { get; }
        public Action<string, Offset> Action { get; }

        public KafkaRegistration(TopicChunk topicChunk, Action<string, Offset> action)
        {
            this.Action = action;
            this.TopicChunk = topicChunk;
        }
    }
}