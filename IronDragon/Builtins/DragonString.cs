// DragonString.cs in EternityChronicles/IronDragon
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
using IronDragon.Runtime;

namespace IronDragon.Builtins
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [DragonExport("String")]
    public class DragonString
    {
        public DragonString()
        {
            Internal = new StringBuilder();
        }

        public DragonString(string @string)
        {
            Internal = new StringBuilder(@string);
        }

        private StringBuilder Internal { get; }

        public override int GetHashCode()
        {
            return Internal.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return Internal.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is DragonString) return ((DragonString)obj).Internal.ToString() == Internal.ToString();
            if (obj is string) return (string)obj == Internal.ToString();
            return false;
        }

        [DragonExport("<<")]
        public void StringAdd(dynamic val)
        {
            Internal.Append((string)val);
        }

        public static implicit operator string(DragonString s)
        {
            return s.Internal.ToString();
        }

        public static implicit operator DragonString(string s)
        {
            return new DragonString(s);
        }
    }
}