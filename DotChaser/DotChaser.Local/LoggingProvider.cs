using HiveShard.Interface.Logging;

namespace DotChaser.Local;

public class LoggingProvider: IHiveShardSimpleLoggingProvider
{
    public void LogDebug(string message)
    {
        Console.WriteLine(message);
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"⚠️ {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine(new Exception(message));
    }
}