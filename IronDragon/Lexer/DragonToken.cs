// DragonToken.cs in EternityChronicles/IronDragon
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

using Antlr4.Runtime;
using Microsoft.Scripting;

namespace IronDragon.Lexer
{
    /// <summary>
    ///     Token used by Dragon, deriving from CommonToken.
    /// </summary>
    public abstract class DragonToken : CommonToken
    {
        protected DragonToken(int type, string text) : base(type, text)
        {
        }

        public SourceSpan Span { get; internal set; }

        public DragonTokenCategory Category { get; protected set; }
    }

    public enum DragonTokenCategory
    {
        Keyword,
        Identifier,
        Number,
        String,
        Comment,
        Normal
    }

    public class DragonToken<T> : DragonToken
    {
        public DragonToken(int type, string text) : this(type, text, default)
        {
        }

        public DragonToken(int type, string text, T value, DragonTokenCategory category = DragonTokenCategory.Normal) :
            base(type, text)
        {
            Value = value;

            Category = category;
        }

        public T Value { get; set; }
    }
}