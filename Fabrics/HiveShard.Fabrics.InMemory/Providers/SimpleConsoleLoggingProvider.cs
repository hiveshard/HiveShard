using System;
using HiveShard.Interface.Logging;

namespace HiveShard.Fabrics.InMemory.Providers;

public class SimpleConsoleLoggingProvider: IHiveShardSimpleLoggingProvider
{
    public void LogDebug(string message)
    {
        Console.WriteLine("DEBUG: "+message);
    }

    public void LogWarning(string message)
    {
        Console.WriteLine("WARNING: "+message);
    }

    public void LogError(string message)
    {
        string actualMessage = "ERROR: " + message;
        Console.Error.WriteLine(actualMessage);
        Console.WriteLine(actualMessage);
    }
}