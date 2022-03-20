// DragonTokenFactory.cs in EternityChronicles/IronDragon
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