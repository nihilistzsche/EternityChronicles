// Channel.cs in EternityChronicles/CSLog
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
                listener.LogMessage(message, level, Name);
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