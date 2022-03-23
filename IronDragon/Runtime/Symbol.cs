// Symbol.cs in EternityChronicles/IronDragon
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

namespace IronDragon.Runtime
{
    /// <summary>
    ///     Symbols are like variable names except they are unique in any given execution environment.
    /// </summary>
    public class Symbol
    {
        private Symbol(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public static Symbol NewSymbol(string name)
        {
            Symbol sym;
            if (DragonScope.Symbols.ContainsKey(name))
            {
                sym = DragonScope.Symbols[name];
            }
            else
            {
                sym = new Symbol(name);
                DragonScope.Symbols[name] = sym;
            }

            return sym;
        }
    }
}