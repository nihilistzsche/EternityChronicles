// Dragon.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IronDragon.Builtins;
using IronDragon.Runtime;
using Microsoft.Scripting.Hosting;

namespace IronDragon
{
    /// <summary>
    ///     Main class for users of Dragon.
    /// </summary>
    public static class Dragon
    {
        private static readonly LanguageSetup DragonSetup =
            new("IronDragon.Runtime.DragonContext,IronDragon,Version=0.5.0.0,Culture=neutral",
                "IronDragon 0.5", new[] { "IronDragon" }, new[] { ".dragon", ".drg" });

        static Dragon()
        {
            Globals = new DragonGlobalScope();
            new Kernel();
        }

        public static DragonGlobalScope Globals { get; }

        /// <summary>
        ///     Generate a FunctionArgument object to be used for named arguments when calling Dragon functions from a .NET
        ///     language.
        /// </summary>
        /// <example>(Where testfunc is a Dragon dynamic function): testfunc(Dragon.Arg("b",2),10);</example>
        /// <param name="name">Name of the argument.</param>
        /// <param name="value">Value for the argument.</param>
        public static FunctionArgument Arg(dynamic name, dynamic value)
        {
            return new FunctionArgument(name.ToString(), Expression.Constant(value));
        }

        /// <summary>
        ///     Returns a DragonNativeFunction invokable wrapper around the Arg() function that allows you to consume Dragon
        ///     keyword arguments from other languages, pursuant to the limitations of your language.
        /// </summary>
        /// <example>scope.SetVariable("sa",Dragon.GetFunctionArgumentGenerator())</example>
        /// <returns>A DragonNativeFuncton dynamic wrapper around Dragon.Arg(name,value).</returns>
        public static dynamic GetFunctionArgumentGenerator()
        {
            return new DragonNativeFunction(typeof(Dragon),
                                            typeof(Dragon).GetMethod("Arg", BindingFlags.Public | BindingFlags.Static));
        }

        public static dynamic Box(object obj, DragonScope scope = null)
        {
            return DragonBoxedInstance.Box(obj, scope ?? new DragonScope());
        }

        public static dynamic BoxNoCache(object obj, DragonScope scope = null)
        {
            return DragonBoxedInstance.BoxNoCache(obj, scope ?? new DragonScope());
        }

        public static ScriptRuntime CreateRuntime(params LanguageSetup[] setups)
        {
            var setup = new ScriptRuntimeSetup();
            setups.ToList().ForEach(lsetup => setup.LanguageSetups.Add(lsetup));
            return new ScriptRuntime(setup);
        }

        public static ScriptRuntime CreateRuntime()
        {
            return CreateRuntime(DragonSetup);
        }

        public static LanguageSetup CreateDragonSetup()
        {
            return DragonSetup;
        }

        public static dynamic Execute(string source, DragonScope scope)
        {
            var xscope = CreateRuntime().GetEngine("IronDragon").CreateScope();
            scope.MergeIntoScope(xscope);
            var val = CreateRuntime().GetEngine("IronDragon").CreateScriptSourceFromString(source).Execute(xscope);
            scope.MergeWithScope(xscope);
            return val;
        }

        public static dynamic Execute(string source)
        {
            return Execute(source, Globals);
        }

        public static dynamic Box(Type type)
        {
            return DragonClass.BoxClass(type);
        }

        public static void SetCurrentDirectory(string currentDir)
        {
            RuntimeOperations.CurrentDir = currentDir;
        }
    }

    public static class DragonExtensions
    {
        public static dynamic Eval(this string source)
        {
            return Dragon.Execute(source);
        }
    }
}