// DragonAbstractTestFixture.cs
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

        public static Symbol XS(string name)
        {
            return Symbol.NewSymbol(name);
        }

        public static DragonDictionary SD(Dictionary<object, object> dict)
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