// DefaultFormatter.cs
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
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace CSLog
{
    public class DefaultFormatter : Formatter
    {
        private static bool _replaceColor = true;

        // ReSharper disable once InconsistentlySynchronizedField
        public static void SetReplaceColor(bool replace)
        {
            _replaceColor = replace;
        }

        public override string FormatString(string input)
        {
            lock (this)
            {
                var now = DateTime.Now;

                var nowDesc = now.ToString("yyyy-MM-dd HH:mm:s");
                var processName = Process.GetCurrentProcess().ProcessName;
                var processId = Process.GetCurrentProcess().Id.ToString();

                var threadIdTemp = Thread.CurrentThread.ManagedThreadId.ToString();
                var bytes = Encoding.UTF8.GetBytes(threadIdTemp);
                var threadId = BitConverter.ToString(bytes).Replace("-", "");

                return ProcessColorInString($"{nowDesc} {processName}[{processId}:{threadId}] {input}", _replaceColor);
            }
        }
    }
}