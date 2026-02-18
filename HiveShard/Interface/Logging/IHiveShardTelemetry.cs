using System;
using HiveShard.Interface.Config;

namespace HiveShard.Interface.Logging;

public interface IHiveShardTelemetry
{
    void LogWarning(string message);
    void LogDebug(string message);
    void LogException(Exception exception);
    IHiveShardTelemetry GetScopedLogger<T>(IIdentityConfig identityConfig);
}