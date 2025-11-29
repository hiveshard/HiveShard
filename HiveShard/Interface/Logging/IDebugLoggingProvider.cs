namespace HiveShard.Interface.Logging
{
    public interface IDebugLoggingProvider: IWarningLoggingProvider
    {
        public void LogDebug(string message);
    }
}