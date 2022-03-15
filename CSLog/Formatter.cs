using System.Collections.Generic;
using System.Linq;

namespace CSLog
{
    public abstract class Formatter
    {
        private static readonly Dictionary<string, string> Colors;

        static Formatter()
        {
            Colors = new Dictionary<string, string>
            {
                { "`k", "\x1B[0;30m" },
                { "`r", "\x1B[0;31m" },
                { "`g", "\x1B[0;32m" },
                { "`y", "\x1B[0;33m" },
                { "`b", "\x1B[0;34m" },
                { "`m", "\x1B[0;35m" },
                { "`c", "\x1B[0;36m" },
                { "`w", "\x1B[0;37m" },
                { "`K", "\x1B[1;30m" },
                { "`R", "\x1B[1;31m" },
                { "`G", "\x1B[1;32m" },
                { "`Y", "\x1B[1;33m" },
                { "`B", "\x1B[1;34m" },
                { "`M", "\x1B[1;35m" },
                { "`C", "\x1B[1;36m" },
                { "`W", "\x1B[1;37m" },
                { "!k", "\x1B[0;40m" },
                { "!K", "\x1B[1;40m" },
                { "!r", "\x1B[0;41m" },
                { "!R", "\x1B[1;41m" },
                { "!g", "\x1B[0;42m" },
                { "!G", "\x1B[1;42m" },
                { "!y", "\x1B[0;43m" },
                { "!Y", "\x1B[1;43m" },
                { "!b", "\x1B[0;44m" },
                { "!B", "\x1B[1;44m" },
                { "!m", "\x1B[0;45m" },
                { "!M", "\x1B[1;45m" },
                { "!c", "\x1B[0;46m" },
                { "!C", "\x1B[1;46m" },
                { "!w", "\x1B[0;47m" },
                { "!W", "\x1B[1;47m" },
                { "`#", "\t" },
                { "`@", "\n\r" },
                { "`x", "\x1B[0m" },
                { "`!", "\x1B[5m" },
                { "#!", "\x1B[25m" }
            };
        }

        public virtual string ProcessColorInString(string input, bool empty = false)
        {
            return Colors.Keys.Aggregate(input, (current, color) => current.Replace(color, empty ? "" : Colors[color]));
        }

        public virtual string FormatString(string input)
        {
            return input;
        }
    }
}