// DragonTokenQueue.cs in EternityChronicles/IronDragon
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

using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using IronDragon.Parser;
using Microsoft.Scripting;

namespace IronDragon.Lexer
{
    public class DragonTokenQueue : ITokenSource
    {
        private readonly DragonTokenFactory _factory;
        private readonly DragonLexer _lexer;
        private readonly Queue<CommonToken> _tokens;

        public DragonTokenQueue(DragonParser parser, DragonLexer lexer, string sourceName)
        {
            Parser = parser;
            _lexer = lexer;
            SourceName = sourceName;
            _factory = new DragonTokenFactory { Parser = parser };
            _tokens = new Queue<CommonToken>();
        }

        internal DragonParser Parser { get; }

        public int NumTokens { get; private set; }

        public ICharStream InputStream => null;

        public string SourceName { get; }

        public ITokenFactory TokenFactory
        {
            get => _factory;
            set { }
        }

        public IToken NextToken()
        {
            return _tokens.Count > 0 ? _tokens.Dequeue() : new CommonToken(-1);
        }

        public int Line => _tokens.First().Line;
        public int Column => _tokens.First().Column;

        public void AddToken<T>(string name, string text, T value,
                                DragonTokenCategory category = DragonTokenCategory.Normal)
        {
            var num = _factory.GetTokenFromName(name);
            if (num == -2) throw new SyntaxErrorException($"Invalid token {name}");
            var item = new DragonToken<T>(num, text, value) { Span = _lexer.GetSourceSpanOfToken() };
            if (category == DragonTokenCategory.Comment) item.Channel = TokenConstants.HiddenChannel;
            NumTokens++;
            _tokens.Enqueue(item);
        }
    }
}