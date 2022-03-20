// DragonContext.cs in EternityChronicles/IronDragon
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

using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Expressions;
using IronDragon.Parser;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class DragonContext : LanguageContext
    {
        public DragonContext(ScriptDomainManager domainManager, IDictionary<string, object> options) : base(
         domainManager)
        {
        }

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options,
                                                     ErrorSink errorSink)
        {
            var res = DragonParser.Parse(sourceUnit.GetCode(), sourceUnit);

            if (res != null)
            {
                Expression mainBlock = DragonExpression.DragonBlock(res);
                return new DragonScriptCode(mainBlock, sourceUnit);
            }

            throw new SyntaxErrorException("Syntax error", sourceUnit, SourceSpan.None, 0, Severity.Error);
        }
    }
}