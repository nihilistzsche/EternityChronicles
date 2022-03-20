// Master.cs in EternityChronicles/CSLog
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
    public enum LogLevel
    {
        Any,
        Debug,
        Info,
        Warning,
        Critical,
        Fatal
    }

    public class Master
    {
        public static readonly Master DefaultMaster = new();

        public Dictionary<LogLevel, List<IListener>> GlobalListeners { get; } = new();

        public Dictionary<string, Channel> Channels { get; } = new();

        public void RegisterChannel(Channel channel)
        {
            Channels[channel.Name] = channel;
            foreach (var key in GlobalListeners.Keys)
            {
                channel.Listeners[key] = new List<IListener>();
                foreach (var listener in GlobalListeners[key]) channel.Listeners[key].Add(listener);
            }
        }

        private static void RegisterListener(Channel channel, IListener listener, List<LogLevel> levels = null)
        {
            if (levels != null)
                foreach (var level in levels)
                    channel.RegisterListener(listener, level);
            else
                channel.RegisterListener(listener, LogLevel.Any);
        }

        public void AddListener(IListener listener, List<string> channels = null, List<LogLevel> levels = null)
        {
            if (channels != null)
                foreach (var channel in channels.Select(channelName => Channels[channelName])
                                                .Where(channel => channel != null))
                    RegisterListener(channel, listener, levels);
            else
                foreach (var channel in Channels.Keys.Select(channelName => Channels[channelName]))
                    RegisterListener(channel, listener, levels);
        }

        public void LogMessage(string message, string channelName, LogLevel level)
        {
            if (!Channels.ContainsKey(channelName)) return;

            var channel = Channels[channelName];
            channel.LogMessage(message, level);
        }
    }

    public static class Log
    {
        public static readonly Master DefaultMaster = Master.DefaultMaster;

        public static void LogMessage(string channel, LogLevel level, string message, params object[] pArgs)
        {
            DefaultMaster.LogMessage(string.Format(message, pArgs), channel, level);
        }

        public static void RegisterChannel(string channelName)
        {
            DefaultMaster.RegisterChannel(new Channel(channelName));
        }
    }
}