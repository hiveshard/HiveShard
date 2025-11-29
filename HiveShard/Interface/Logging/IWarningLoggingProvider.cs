using System.Runtime.CompilerServices;

namespace HiveShard.Interface.Logging;

public interface IWarningLoggingProvider
{
    public void LogWarning(string warning, [CallerMemberName] string name = "");
}