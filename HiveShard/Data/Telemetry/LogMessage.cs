namespace HiveShard.Data.Telemetry
{
    public class LogMessage
    {
        public LogMessage(string message, LogLevel logLevel, LogOrigin logOrigin, long tick)
        {
            Message = message;
            LogLevel = logLevel;
            LogOrigin = logOrigin;
            Tick = tick;
        }

        public string Message { get; }
        public LogLevel LogLevel { get; }
        public LogOrigin LogOrigin { get; }
        public long Tick { get; }
    }
}