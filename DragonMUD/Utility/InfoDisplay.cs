using System;
using System.Linq;
using System.Text;

namespace DragonMUD.Utility
{
    public class InfoDisplay : BaseObject
    {
        public InfoDisplay()
        {
            Display  = new StringBuilder();
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
                var    components = line.Split(' ');
                var    tmpLine    = new StringBuilder();
                var    i          = 1;
                string areWeTooLong;
                string areWeTooLongNC;
                tmpLine.Append(components[0]);
                int o;
                OldColor = "";
                do
                {
                    o              = i;
                    areWeTooLong   = $"{tmpLine} {components[i]}";
                    areWeTooLongNC = hook.ProcessMessage(areWeTooLong, false);
                    if (areWeTooLongNC.Length + 4 < 80)
                    {
                        var c                        = components[i++];
                        var location                 = c.IndexOf("`", StringComparison.InvariantCulture);
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