using System;
using HiveShard.Data.Telemetry;
using HiveShard.Interface.Logging;
using Xcepto.Interfaces;

namespace Xcepto.HiveShard.Providers
{
    public class LoggingProvider : ILoggingProvider, IWorkerLoggingProvider, IHiveShardSimpleLoggingProvider, IDebugLoggingProvider
    {
        public void LogDebug(string message, LogOrigin logOrigin) => LogDebug(message);

        public void LogDebug(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message, LogOrigin logOrigin) => LogWarning(message);
        public void LogWarning(string message)
        {
            Console.WriteLine($"\x1b[33m{message}\x1b[0m");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"\x1b[31m{message}\x1b[0m");
            Console.Error.WriteLine($"\x1b[31m{message}\x1b[0m");
        }

        public void LogError(string message, LogOrigin logOrigin) => LogError(message);
        public void LogError(Exception exception, LogOrigin logOrigin) => LogError(exception.ToString());
    }
}