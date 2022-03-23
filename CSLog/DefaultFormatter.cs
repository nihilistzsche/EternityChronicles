// DefaultFormatter.cs in EternityChronicles/CSLog
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