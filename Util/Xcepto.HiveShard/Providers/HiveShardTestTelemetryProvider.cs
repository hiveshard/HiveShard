using System.Collections.Concurrent;
using HiveShard.Interface.Logging;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard.Providers;

public class HiveShardTestTelemetryProvider : ILoggingProvider
{
    private readonly IHiveShardTelemetry _telemetry;
        
    private readonly ConcurrentQueue<string> _messages = new();

    public HiveShardTestTelemetryProvider(IHiveShardTelemetry telemetry)
    {
        _telemetry = telemetry;
    }

    public void LogDebug(string message)
    {
        _messages.Enqueue(message);
    }

    public void Flush()
    {
        while (!_messages.IsEmpty)
            if (_messages.TryDequeue(out string message)) _telemetry.LogDebug(message);
    }

    public void Dispose() => Flush();
}