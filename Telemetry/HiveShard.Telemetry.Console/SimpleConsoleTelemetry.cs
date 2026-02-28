using System;
using HiveShard.Interface.Config;
using HiveShard.Interface.Logging;

namespace HiveShard.Telemetry.Console
{
    public class SimpleConsoleTelemetry: IHiveShardTelemetry
    {
        public void LogWarning(string message)
        {
            System.Console.WriteLine($"[WARNING] {message}");
        }

        public void LogDebug(string message)
        {
            System.Console.WriteLine($"[DEBUG] {message}");
        }

        public void LogException(Exception exception)
        {
            System.Console.WriteLine($"[ERROR] {exception}");
            System.Console.Error.WriteLine(exception);
        }

        public IHiveShardTelemetry GetScopedLogger<T>(IIdentityConfig identityConfig)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}