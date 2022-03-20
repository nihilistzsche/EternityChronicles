// Channel.cs
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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