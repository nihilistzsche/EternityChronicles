// DragonLexer.partial.cs in EternityChronicles/IronDragon
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IronDragon.Parser;
using Microsoft.Scripting;

#pragma warning disable 162

namespace IronDragon.Lexer
{
    /// <summary>
    ///     Extra methods for lexer
    /// </summary>
    public partial class DragonLexer
    {
        private readonly Dictionary<int, int> _lineIndexes = new();
        private string[] _lines;

        public string CreateString()
        {
            return data.Substring(ts, te - ts);
        }

        public void Token(string name, DragonTokenCategory category = DragonTokenCategory.Normal)
        {
            var text = CreateString();
            Queue.AddToken(name, text, text);
        }

        public void Token<T>(string name, T value, DragonTokenCategory category = DragonTokenCategory.Normal)
        {
            Queue.AddToken(name, CreateString(), value);
        }

        public void Keyword(string name)
        {
            Token(name, DragonTokenCategory.Keyword);
        }

        public void Symbol(string name)
        {
            Token(name, DragonTokenCategory.Identifier);
        }

        public void Number()
        {
            var text = CreateString();
            Queue.AddToken("NUMBER", text, double.Parse(text), DragonTokenCategory.Number);
        }

        public void Integer()
        {
            var text = CreateString();
            Queue.AddToken("INTEGER", text, int.Parse(text), DragonTokenCategory.Number);
        }

        public void Hex()
        {
            var text = CreateString();
            Queue.AddToken("INTEGER", text, int.Parse(text.Substring(2), NumberStyles.HexNumber),
                           DragonTokenCategory.Number);
        }

        public void String()
        {
            var text = CreateString();
            Queue.AddToken("STRING", text, text.Substring(1, text.Length - 2), DragonTokenCategory.String);
        }

        public void Regex()
        {
            var text = CreateString();
            Queue.AddToken("REGEX", text, text.Substring(2, text.Length - 3), DragonTokenCategory.String);
        }

        public void CustomOp()
        {
            var text = CreateString();
            Queue.AddToken("OP", text, text);
        }

        public void SetLines(DragonParser parser)
        {
            parser.Lines = _lines;
        }

        private void SetupLineIndexes(string source)
        {
            _lines = source.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.None);
            _lineIndexes[0] = 0;
            for (var i = 1; i < _lines.Length; i++) _lineIndexes[i] = _lines[i - 1].Length;
        }

        internal SourceSpan GetSourceSpanOfToken()
        {
            var ls = _lineIndexes.Where(k => k.Value > ts).Select(k => k.Key).FirstOrDefault();
            var le = _lineIndexes.Where(k => k.Value > te).Select(k => k.Key).FirstOrDefault();

            var start = new SourceLocation(Queue.NumTokens, ls + 1, Math.Max(ts - _lineIndexes[ls] + 1, 1));

            var end = new SourceLocation(Queue.NumTokens, le + 1, Math.Max(te - _lineIndexes[le] + 1, 1));
            return new SourceSpan(start, end);
        }
    }
}