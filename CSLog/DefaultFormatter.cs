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
        public static void SetReplaceColor(bool replace) => _replaceColor = replace;

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
