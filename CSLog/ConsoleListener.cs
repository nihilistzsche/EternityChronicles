// ConsoleListener.cs
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