using System.Text;

namespace DragonMUD.Utility
{
    public static class DragonMUDStringExtensions
    {
        public static string GetSpacing(this string @this)
        {
            var hook = new ColorProcessWriteHook();

            // ReSharper disable once ConvertToLocalFunction
            string getStringSpacing(string s)
            {
                var spacing = new StringBuilder();
                var clrString = hook.ProcessMessage(s, false);
                var i = clrString.Length;
                while (i++ < 78) spacing.Append(" ");

                return spacing.ToString();
            }

            return getStringSpacing(@this);
        }
    }
}