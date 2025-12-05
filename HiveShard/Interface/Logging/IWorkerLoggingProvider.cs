using System;
using HiveShard.Data.Telemetry;

namespace HiveShard.Interface.Logging
{
    public interface IWorkerLoggingProvider: IWarningLoggingProvider
    {
        void LogDebug(string message, LogOrigin logOrigin);
        void LogDebug(string message);
        void LogWarning(string message, LogOrigin logOrigin);
        void LogWarning(string message);
        void LogError(string message, LogOrigin logOrigin);
        void LogError(Exception exception, LogOrigin logOrigin);
    }
}