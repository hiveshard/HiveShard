using System;
using HiveShard.Data;
using HiveShard.Interface.Config;

namespace HiveShard.Interface.Logging;

public interface IHiveShardTelemetry: IDisposable
{
    void LogWarning(string message);
    void LogDebug(string message);
    void LogException(Exception exception);
    void Cause(TransitionCause cause);
    IHiveShardTelemetry GetScopedLogger<T>(IIdentityConfig identityConfig);
}