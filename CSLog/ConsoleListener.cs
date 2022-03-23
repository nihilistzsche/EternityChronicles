// ConsoleListener.cs in EternityChronicles/CSLog
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