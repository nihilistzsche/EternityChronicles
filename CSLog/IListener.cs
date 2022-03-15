namespace CSLog
{
    public interface IListener
    {
        void SetFormatter(Formatter formatter);

        void LogMessage(string message, LogLevel level, string channelName);
    }
}