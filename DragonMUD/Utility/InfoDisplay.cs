// InfoDisplay.cs in EternityChronicles/DragonMUD
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
using System.Linq;
using System.Text;

namespace DragonMUD.Utility
{
    public class InfoDisplay : BaseObject
    {
        public InfoDisplay()
        {
            Display = new StringBuilder();
            OldColor = "";
        }

        public StringBuilder Display { get; }

        public string FinalOutput => Display.ToString();

        private string OldColor { get; set; }

        public void ClearScreen()
        {
            Display.Append("{!}");
        }

        public void AppendSeparater()
        {
            Display.Append("`w+------------------------------------------------------------------------------+`x\n\r");
        }

        public void AppendLine(string line)
        {
            var hook = new ColorProcessWriteHook();
            if (hook.ProcessMessage(line, false).Length + 4 >= 80)
            {
                var components = line.Split(' ');
                var tmpLine = new StringBuilder();
                var i = 1;
                string areWeTooLong;
                string areWeTooLongNC;
                tmpLine.Append(components[0]);
                int o;
                OldColor = "";
                do
                {
                    o = i;
                    areWeTooLong = $"{tmpLine} {components[i]}";
                    areWeTooLongNC = hook.ProcessMessage(areWeTooLong, false);
                    if (areWeTooLongNC.Length + 4 < 80)
                    {
                        var c = components[i++];
                        var location = c.IndexOf("`", StringComparison.InvariantCulture);
                        if (location != -1) OldColor = c.Substring(location, 2);
                        tmpLine.Append($" {c}");
                    }
                } while (o != i && i < components.Count());

                var lineToAppend = $"`w| {tmpLine}";
                lineToAppend = $"{lineToAppend}{lineToAppend.GetSpacing()}`w |`x\n\r";
                Display.Append(lineToAppend);
                var rest = string.Join(" ", components.Skip(i).Take(components.Count() - i));
                AppendLine($"  {OldColor}{rest}");
            }
            else
            {
                var lineToAppend = $"`w| {OldColor}{line}";
                lineToAppend = $"${lineToAppend}{lineToAppend.GetSpacing()}`w |`x\n\r";
                Display.Append(lineToAppend);
            }
        }

        public void AppendString(string str)
        {
            Display.Append(str);
        }
    }
}