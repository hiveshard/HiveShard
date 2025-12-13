using System;
using HiveShard.Data.Telemetry;
using HiveShard.Interface.Logging;

namespace HiveShard.Deployments.InMemory.Providers;

public class SimpleLoggingProvider: IWorkerLoggingProvider, IHiveShardSimpleLoggingProvider, IDebugLoggingProvider
{
    public void LogDebug(string message, LogOrigin logOrigin)
    {
        Console.WriteLine(message);
    }

    public void LogDebug(string message)
    {
        Console.WriteLine(message);
    }

    public void LogWarning(string message, LogOrigin logOrigin)
    {
        Console.WriteLine(message);
    }

    public void LogWarning(string message)
    {
        Console.WriteLine(message);
    }

    public void LogError(string message)
    {
        Console.WriteLine(message);
    }

    public void LogError(string message, LogOrigin logOrigin)
    {
        Console.WriteLine(message);
    }

    public void LogError(Exception exception, LogOrigin logOrigin)
    {
        Console.WriteLine(exception.Message);
    }

    public void LogWarning(string warning, string name = "")
    {
        LogWarning(warning);
    }
}