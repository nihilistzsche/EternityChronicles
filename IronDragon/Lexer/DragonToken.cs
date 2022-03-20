// DragonToken.cs in EternityChronicles/IronDragon
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