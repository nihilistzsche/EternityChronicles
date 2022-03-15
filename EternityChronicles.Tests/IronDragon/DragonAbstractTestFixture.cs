// -----------------------------------------------------------------------
// <copyright file="DragonAbstractTestFixture.cs" company="Microsoft">
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