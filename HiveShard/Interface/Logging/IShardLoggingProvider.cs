namespace HiveShard.Interface.Logging
{
    public interface IShardLoggingProvider
    {
        public void LogDebug(string message);
        void Warning(string message);
    }
}