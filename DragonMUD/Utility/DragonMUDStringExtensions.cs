// DragonMUDStringExtensions.cs in EternityChronicles/DragonMUD
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