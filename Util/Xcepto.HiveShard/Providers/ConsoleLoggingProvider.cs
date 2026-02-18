using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using HiveShard.Data.Telemetry;
using HiveShard.Interface.Logging;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard.Providers
{
    public class ConsoleLoggingProvider : ILoggingProvider, IWorkerLoggingProvider, IHiveShardSimpleLoggingProvider, IDebugLoggingProvider
    {
        private ConcurrentQueue<string> _messages = new();
        public void LogDebug(string message, LogOrigin logOrigin) => LogDebug(message);

        public void LogDebug(string message)
        {
            _messages.Enqueue(message);
        }

        public void Flush()
        {
            while (!_messages.IsEmpty)
            {
                if (_messages.TryDequeue(out string message))
                {
                    Console.WriteLine(message);
                }
            }
        }

        public void LogWarning(string message, LogOrigin logOrigin) => LogWarning(message);
        public void LogWarning(string message)
        {
            _messages.Enqueue(message);
        }

        public void LogError(string message)
        {
            _messages.Enqueue(message);
            Console.Error.WriteLine(message);
        }

        public void LogError(string message, LogOrigin logOrigin) => LogError(message);
        public void LogError(Exception exception, LogOrigin logOrigin) => LogError(exception.ToString());
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public void LogWarning(string warning, [CallerMemberName] string name = "") => LogWarning(warning);

        public void Dispose() => Flush();
    }
}