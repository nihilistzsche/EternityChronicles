// DragonTokenFactory.cs in EternityChronicles/IronDragon
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
using Antlr4.Runtime;
using IronDragon.Parser;

namespace IronDragon.Lexer
{
    public class DragonTokenFactory : ITokenFactory
    {
        internal DragonParser Parser { private get; set; }

        public IToken Create(int type, string text)
        {
            return new DragonToken<string>(type, text);
        }

        public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start,
                             int stop, int line, int charPositionInLine)
        {
            return Create(type, text);
        }

        public int GetTokenFromName(string name)
        {
            var klass = typeof(DragonParser);
            try
            {
                var field = klass.GetField(name);

                return (int)field.GetValue(Parser);
            }
            catch (Exception)
            {
                return -2;
            }
        }
    }
}