// DragonContext.cs in EternityChronicles/IronDragon
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