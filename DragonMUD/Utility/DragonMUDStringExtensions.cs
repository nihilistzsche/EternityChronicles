// DragonMUDStringExtensions.cs in EternityChronicles/DragonMUD
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

using System.Text;

namespace DragonMUD.Utility
{
    public static class DragonMudStringExtensions
    {
        public static string GetSpacing(this string @this)
        {
            var hook = new ColorProcessWriteHook();

            // ReSharper disable once ConvertToLocalFunction
            string GetStringSpacing(string s)
            {
                var spacing = new StringBuilder();
                var clrString = hook.ProcessMessage(s, false);
                var i = clrString.Length;
                while (i++ < 78) spacing.Append(" ");

                return spacing.ToString();
            }

            return GetStringSpacing(@this);
        }
    }
}