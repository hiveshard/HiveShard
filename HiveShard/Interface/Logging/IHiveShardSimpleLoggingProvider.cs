namespace HiveShard.Interface.Logging
{
    public interface IHiveShardSimpleLoggingProvider
    {
        public void LogDebug(string message);
        public void LogWarning(string message);
        public void LogError(string message);
    }
}