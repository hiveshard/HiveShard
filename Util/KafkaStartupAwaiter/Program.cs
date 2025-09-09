using System.Diagnostics;
using Confluent.Kafka;
using Polly;

namespace KafkaStartupAwaiter;

class Program
{
    private const int Timeout = 30;
    
    static async Task<int> Main(string[] args)
    {
        var totalTimeout = TimeSpan.FromSeconds(Timeout);
        var sw = Stopwatch.StartNew();

        var policy = Policy
            .Handle<Exception>()
            .RetryForeverAsync(async x =>
            {
                if (sw.Elapsed >= totalTimeout)
                    throw x;

                await Task.Delay(500);
            });

        string broker = Environment.GetEnvironmentVariable("KAFKA_ENDPOINT") ?? "localhost:9092";
        ProducerConfig producerConfig = new ProducerConfig
        {
            BootstrapServers = broker,
            MessageTimeoutMs = 1000,
            SocketTimeoutMs = 1000,
        };

        try
        {
            await policy.ExecuteAsync(async () =>
            {
                try
                {
                    using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();

                    var destination = new TopicPartition("test", new Partition(0));
                    var delivery = await producer.ProduceAsync(destination, new Message<Null, string> { Value = "test" });
                    if (delivery.Status == PersistenceStatus.NotPersisted)
                        throw new Exception("Message not delivered");
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });

            return 0;
        }
        catch
        {
            Console.WriteLine($"Failed after {Timeout}s of retries.");
            return 1;
        }
    }
}