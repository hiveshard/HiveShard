using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using HiveShard.Data;
using HiveShard.Interface;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;
using HiveShard.Shard.Data;
using HiveShard.Util;
using HiveShard.Worker.Data;

namespace HiveShard.Fabrics.Kafka
{
    public class KafkaTopicConsumer
    {
        private IAdminClient? _adminClient;
        private string _topic;
        private ConsumerConfig _consumerConfig;
        private IWorkerLoggingProvider _loggingProvider;
        private CancellationToken _token;
        private IConsumer<Ignore, string>? _consumer;
        private AdminClientConfig _adminClientConfig;

        public KafkaTopicConsumer(string broker, string topic, int partition, IWorkerLoggingProvider loggingProvider,
            CancellationToken token, IFabricLoggingProvider fabricLoggingProvider, IIdentityConfig identityConfig)
        {
            _partition = partition;
            _topic = topic;
            _token = token;
            _loggingProvider = loggingProvider;
            _scopedFabricLoggingProvider = fabricLoggingProvider.GetScopedLogger<KafkaTopicConsumer>(identityConfig);
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = broker,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                GroupId = $"worker-{topic}-{partition}-{Guid.NewGuid().ToString()}",
            };
            _adminClientConfig = new AdminClientConfig { BootstrapServers = broker };
        }

        private Dictionary<string, List<Serving>> _servings = new();
        public void HookUp<T>(Serving serving)
        {
            var typeName = typeof(T).FullName;
            if (typeName is null)
                throw new Exception("type was null");
            if (!_servings.ContainsKey(typeName))
                _servings[typeName] = new List<Serving>();
        
            _servings[typeName].Add(serving);
        }

        private bool _running;
        private int _partition;
        private IScopedFabricLoggingProvider _scopedFabricLoggingProvider;

        public Task Start()
        {
            if (_running)
                throw new Exception("Consumer already running");
            _running = true;
            return Task.Run(async () =>
            {
                _consumer = null;
                await Resilience.Retry(async _ =>
                {
                    await EnsureTopicExists(_topic);
                    EnsureConsumerExists();
                    if (_consumer is null)
                        throw new Exception();
                    // _consumer.Subscribe(_topic); // no more auto partition assignment
                    _consumer.Assign(new Confluent.Kafka.TopicPartition(_topic, new Partition(_partition)));
                }, "Subscribe", CancellationToken.None, _scopedFabricLoggingProvider);

                while (!_token.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(_token);
                        var messageTypeHeader = result.Message.Headers.FirstOrDefault(x => x.Key == "message-type");
                        if (messageTypeHeader is null)
                            throw new Exception("no message type");
                        var messageType = System.Text.Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());
                        if (!_servings.ContainsKey(messageType))
                            throw new Exception($"Message of type {messageType} is not yet consumed");
                    
                        foreach (var serving in _servings[messageType])
                        {
                            serving.DepositBlocking(result.Message.Value, result.Offset.Value);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        _loggingProvider.LogDebug($"Kafka error: {e.Error.Reason}");
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        _loggingProvider.LogDebug("🛑 Kafka consumer cancelled.");
                        break;
                    }
                    catch (Exception e)
                    {
                        _loggingProvider.LogDebug($"💥 Unexpected error: {e}");
                        throw;
                    }
                }

                _consumer.Close();
            }, _token);
        }
        
        
        async Task EnsureTopicExists(string? topic)
        {
            EnsureAdminClientExists();
            try
            {
                if (_adminClient is null)
                    throw new Exception();
                await _adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                    new() { Name = topic, NumPartitions = _partition == 0? 1 : Chunk.MaxChunks, ReplicationFactor = 1 }
                });
                _loggingProvider.LogDebug($"✅ Created topic: {topic}");
            }
            catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
            {
                _loggingProvider.LogWarning("⚠️ Topic already exists, skipping.");
            }
        }

        private void EnsureAdminClientExists()
        {
            try
            {
                _adminClient ??= new AdminClientBuilder(_adminClientConfig)
                    .SetLogHandler((_, message) => throw new Exception($"{message.Level}: {message.Name}, {message.Message}"))
                    .Build();
            }
            catch (Exception)
            {
                _adminClient = null;
                throw;
            }
        }
        private void EnsureConsumerExists()
        {
            try
            {
                _consumer ??= new ConsumerBuilder<Ignore, string>(_consumerConfig)
                    .SetLogHandler((_, message) => throw new Exception($"{message.Level}: {message.Name}, {message.Message}"))
                    .Build();
            }
            catch (Exception)
            {
                _consumer = null;
                throw;
            }
        }
    
    }
}