// DragonAbstractTestFixture.cs in EternityChronicles/EternityChronicles.Tests
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

// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Builtins;
using IronDragon.Expressions;
using IronDragon.Parser;
using IronDragon.Runtime;
using IronRuby;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using DragonC = IronDragon.Dragon;

namespace EternityChronicles.Tests.IronDragon
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [TestFixture]
    public abstract class DragonAbstractTestFixture
    {
        public static ScriptRuntime GetRuntime()
        {
            LanguageSetup jintSetup = new("IronJint.Runtime.JavaScriptContext, IronJint", "IronJint",
                                          new[] { "IronJint" }, new[] { ".js" });
            return DragonC.CreateRuntime(DragonC.CreateDragonSetup(), Ruby.CreateRubySetup(), jintSetup);
        }

        public dynamic CompileAndExecute(string rawsource)
        {
            var engine = GetRuntime().GetEngine("IronDragon");
            var source = engine.CreateScriptSourceFromString(rawsource);
            return source.Execute(engine.CreateScope());
        }

        public static Symbol Xs(string name)
        {
            return Symbol.NewSymbol(name);
        }

        public static DragonDictionary Sd(Dictionary<object, object> dict)
        {
            return new DragonDictionary(dict);
        }


        public Expression Compile(string rawsource)
        {
            var res = DragonParser.Parse(rawsource);

            return res != null ? DragonExpression.DragonBlock(res) : null;
        }
    }
}