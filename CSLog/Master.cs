// Master.cs in EternityChronicles/CSLog
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