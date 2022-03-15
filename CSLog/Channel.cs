using System.Collections.Generic;
using System.Linq;

namespace CSLog
{
    public class Channel
    {
        public Channel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Dictionary<LogLevel, List<IListener>> Listeners { get; } = new();

        public void LogMessage(string message, LogLevel level)
        {
            var levels = new List<LogLevel> { LogLevel.Any };
            if (level != LogLevel.Any) levels.Add(level);

            foreach (var listener in levels.Where(ilevel => Listeners.ContainsKey(ilevel))
                                           .Select(ilevel => Listeners[ilevel])
                                           .SelectMany(listeners => listeners))
            {
                listener.LogMessage(message, level, Name);
            }
        }

        public void RegisterListener(IListener listener, LogLevel level)
        {
            if (!Listeners.ContainsKey(level)) Listeners[level] = new List<IListener>();

            if (level == LogLevel.Any)
            {
                if (Listeners[level].Contains(listener)) return;
            }
            else
            {
                if (Listeners.ContainsKey(LogLevel.Any) && Listeners[LogLevel.Any].Contains(listener)) return;

                if (Listeners[level].Contains(listener)) return;
            }

            Listeners[level].Add(listener);
        }
    }
}