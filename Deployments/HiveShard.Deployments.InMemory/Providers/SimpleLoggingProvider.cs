using System;
using HiveShard.Data.Telemetry;
using HiveShard.Interface.Logging;

namespace HiveShard.Deployments.InMemory.Providers;

public class SimpleLoggingProvider: IWorkerLoggingProvider, IHiveShardSimpleLoggingProvider, IDebugLoggingProvider
{
    public void LogDebug(string message, LogOrigin logOrigin)
    {
        WriteColored($"[DEBUG] [{logOrigin}] {message}", ConsoleColor.Gray);
    }

    public void LogDebug(string message)
    {
        WriteColored($"[DEBUG] {message}", ConsoleColor.Gray);
    }
    public void LogWarning(string message, LogOrigin logOrigin)
    {
        WriteColored($"[WARN][{logOrigin}] {message}", ConsoleColor.Yellow);
    }

    public void LogWarning(string message)
    {
        WriteColored($"[WARN] {message}", ConsoleColor.Yellow);
    }

    public void LogWarning(string warning, string name = "")
    {
        LogWarning(warning);
    }

    public void LogError(string message)
    {
        WriteColored($"[ERROR] {message}", ConsoleColor.Red);
    }

    public void LogError(string message, LogOrigin logOrigin)
    {
        WriteColored($"[ERROR][{logOrigin}] {message}", ConsoleColor.Red);
    }

    public void LogError(Exception exception, LogOrigin logOrigin)
    {
        WriteColored($"[ERROR][{logOrigin}] {exception}", ConsoleColor.Red);
    }

    
    private void WriteColored(string message, ConsoleColor color)
    {
        string ansiColor = color switch
        {
            ConsoleColor.Gray or ConsoleColor.White => "\u001b[37m",
            ConsoleColor.Red                      => "\u001b[31m",
            ConsoleColor.Yellow                   => "\u001b[33m",
            _                                     => "\u001b[37m"
        };

        const string reset = "\u001b[0m";

        Console.WriteLine($"{ansiColor}{message}{reset}");
    }


}