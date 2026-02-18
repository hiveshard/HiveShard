using System;
using HiveShard.Data.Telemetry;
using HiveShard.Interface.Logging;

namespace HiveShard.Deployments.InMemory.Providers;

public class SimpleLoggingProvider: IWorkerLoggingProvider, IHiveShardSimpleLoggingProvider, IDebugLoggingProvider
{
    public void LogDebug(string message, LogOrigin logOrigin)
    {
        Write($"[DEBUG] [{logOrigin}] {message}");
    }

    public void LogDebug(string message)
    {
        Write($"[DEBUG] {message}");
    }
    public void LogWarning(string message, LogOrigin logOrigin)
    {
        Write($"[WARN][{logOrigin}] {message}");
    }

    public void LogWarning(string message)
    {
        Write($"[WARN] {message}");
    }

    public void LogWarning(string warning, string name = "")
    {
        LogWarning(warning);
    }

    public void LogError(string message)
    {
        Write($"[ERROR] {message}");
    }

    public void LogError(string message, LogOrigin logOrigin)
    {
        Write($"[ERROR][{logOrigin}] {message}");
    }

    public void LogError(Exception exception, LogOrigin logOrigin)
    {
        Write($"[ERROR][{logOrigin}] {exception}");
    }

    
    private void Write(string message)
    {
        Console.WriteLine($"{message}");
    }


}