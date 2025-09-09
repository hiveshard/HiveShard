using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using HiveShard.Data;
using HiveShard.Fabric.Ticker;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Util;
using TopicPartition = HiveShard.Data.TopicPartition;

namespace HiveShard.Fabrics.Kafka
{
    public class SimpleKafkaFabric: ISimpleFabric
    {
        private readonly CancellationToken _ctsToken;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ProducerConfig _producerConfig;
        private readonly string _broker;
        private ISerializer _serializer;
        private IScopedFabricLoggingProvider _scopedLogger;
        private IEnvironmentConfig _environmentConfig;
        private BlockingCollection<TopicPartition> _ensureTopics = new();
        private ConcurrentQueue<KafkaRegistration> _kafkaRegistrations = new();
        private ConcurrentQueue<TopicPartition> _newProducers = new();
        private ConcurrentDictionary<TopicPartition, BlockingCollection<string>> _messagesToBeProduced = new();
        public SimpleKafkaFabric(IIdentityConfig identityConfig, ICancellationProvider cancellationProvider, ISerializer serializer, IFabricLoggingProvider fabricLoggingProvider, IEnvironmentConfig environmentConfig)
        {
            _serializer = serializer;
            _environmentConfig = environmentConfig;
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
        
        public void Register<T>(string topic, Action<Consumption<T>> action)
            => Register(topic, new Chunk(0, 0), action);

        public void Register<T>(string topic, Chunk chunk, Action<Consumption<T>> action)
        {
            _ensureTopics.Add(new TopicPartition(topic, chunk), _ctsToken);
            _kafkaRegistrations.Enqueue(new KafkaRegistration(new TopicPartition(topic, chunk), (json, offset) =>
            {
                var message = _serializer.Deserialize<T>(json);
                action(new Consumption<T>(message, offset));
            }));
        }
        
        public async Task Send<T>(string topic, T message) => await Send(topic, new Chunk(0,0), message);
        public Task Send<T>(string topic, Chunk chunk, T message)
        {
            TopicPartition topicPartition = new TopicPartition(topic, chunk);
            _messagesToBeProduced.AddOrUpdate(topicPartition,
                key =>
                {
                    _newProducers.Enqueue(topicPartition);
                    _ensureTopics.Add(topicPartition, _ctsToken);
                    return new BlockingCollection<string>();
                },
                (key, oldValue) => oldValue is null ? new BlockingCollection<string>() : oldValue);
            _messagesToBeProduced[topicPartition].Add(_serializer.Serialize(message));
            return Task.CompletedTask;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(CreateAdminClient(cancellationToken));
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_kafkaRegistrations.TryDequeue(out var kafkaRegistration))
                    {
                        tasks.Add(CreateConsumer(kafkaRegistration));
                    }

                    if (_newProducers.TryDequeue(out var newProducer))
                    {
                        tasks.Add(CreateProducer(newProducer, cancellationToken));
                    }
                    await Task.Delay(100, cancellationToken);
                }
                
                // after cancellation, propagate
                return Task.WhenAll(tasks);
            }, cancellationToken);
        }

        private Task CreateAdminClient(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                IAdminClient adminClient = null;
                await Resilience.Retry(async token =>
                    {
                        if (adminClient is null)
                        {
                            adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _broker })
                                .SetLogHandler((_, logMessage) =>
                                {
                                    _scopedLogger.LogDebug($"{logMessage.Name}: {logMessage.Message}");
                                    if (logMessage.Level <= SyslogLevel.Error)
                                        throw new Exception($"{logMessage.Level}: {logMessage.Name}, {logMessage.Message}");
                                })
                                .Build();
                        }
                    }, "Create AdminClient", _ctsToken, _scopedLogger);
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    var topicPartition = _ensureTopics.Take();
                    var topic = topicPartition.Prefixed(_environmentConfig);
                    await adminClient.CreateTopicsAsync(new TopicSpecification[]
                    {
                        new() { Name = topic, NumPartitions = Chunk.MaxChunks, ReplicationFactor = 1 }
                    });
                    _scopedLogger.LogDebug($"Created topic {topic} with partitions: 1-{Chunk.MaxChunks}");
                }
            }, cancellationToken);
        }

        private Task CreateProducer(TopicPartition topicPartition, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                IProducer<Null, string> producer = null;
                await Resilience.Retry(async token =>
                {
                    if (producer is null)
                    {
                        producer = new ProducerBuilder<Null, string>(_producerConfig)
                            .SetLogHandler((_, logMessage) =>
                            {
                                if (logMessage.Level <= SyslogLevel.Error)
                                    _scopedLogger.LogException(new Exception($"{logMessage.Level}: {logMessage.Name}, {logMessage.Message}"));
                                else
                                    _scopedLogger.LogDebug($"{logMessage.Name}: {logMessage.Message}");
                            })
                            .Build();
                    }
                }, $"Publish {topicPartition.Topic}", _ctsToken, _scopedLogger);
                
                
                var blockingCollection = _messagesToBeProduced[topicPartition];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = blockingCollection.Take();
                    var delivery = await producer.ProduceAsync(new Confluent.Kafka.TopicPartition($"{_environmentConfig.Prefix}-{topicPartition.Topic}", new Partition(topicPartition.Chunk.ToPartition())), 
                        new Message<Null, string> { Value = message, Headers = new Headers()
                        {
                            new Header("message-type", System.Text.Encoding.UTF8.GetBytes(topicPartition.Topic))
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
                    var partition = new Partition(kafkaRegistration.TopicPartition.Chunk.ToPartition());
                    var kafkaTopicPartition = new Confluent.Kafka.TopicPartition(kafkaRegistration.TopicPartition.Prefixed(_environmentConfig), partition);
                    consumer.Assign(kafkaTopicPartition);
                    return Task.CompletedTask;
                    
                }, "Subscribe", _ctsToken, _scopedLogger);
                
                while (!_ctsToken.IsCancellationRequested)
                {
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
                }

                consumer?.Close();
            }, _ctsToken);
        }
        
        public Task WaitForReady() => Task.CompletedTask;
    }

    public class KafkaRegistration
    {
        public TopicPartition TopicPartition { get; }
        public Action<string, Offset> Action { get; }

        public KafkaRegistration(TopicPartition topicPartition, Action<string, Offset> action)
        {
            this.Action = action;
            this.TopicPartition = topicPartition;
        }
    }
}