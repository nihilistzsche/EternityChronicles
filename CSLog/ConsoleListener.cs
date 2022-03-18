using System;
using System.Text;

namespace CSLog
{
    public class ConsoleListener : IListener
    {
        public Formatter Formatter { get; private set; }

        public void SetFormatter(Formatter formatter)
        {
            Formatter = formatter;
        }

        public void LogMessage(string message, LogLevel level, string channelName)
        {
            var stream = level == LogLevel.Fatal || level == LogLevel.Critical
                             ? Console.OpenStandardError()
                             : Console.OpenStandardOutput();

            var formatter = Formatter ?? new DefaultFormatter();

            var colorForMessage = "`w";

            switch (level)
            {
            case LogLevel.Fatal:
                colorForMessage = "`R";
                break;
            case LogLevel.Critical:
                colorForMessage = "`r";
                break;
            case LogLevel.Warning:
                colorForMessage = "`y";
                break;
            case LogLevel.Info:
                colorForMessage = "`w";
                break;
            case LogLevel.Debug:
                colorForMessage = "`g";
                break;
            case LogLevel.Any:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            var temp = $"{colorForMessage}{message}";
            var imessage = formatter.FormatString($"`w[`c{channelName}`w] {temp}`x\n");

            var bytes = Encoding.UTF8.GetBytes(imessage);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}