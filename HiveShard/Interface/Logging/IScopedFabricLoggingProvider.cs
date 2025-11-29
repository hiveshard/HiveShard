using System;
using System.Runtime.CompilerServices;

namespace HiveShard.Interface.Logging
{
    public interface IScopedFabricLoggingProvider: IWarningLoggingProvider
    {
        public void LogDebug(string message, [CallerMemberName] string name = "");
        public void LogException(Exception exception, [CallerMemberName] string name = "");
    }
}