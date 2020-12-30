// -----------------------------------------------------------------------
// <copyright file="DynamicScope.cs" Company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IronDragon;
using IronDragon.Builtins;
using IronDragon.Runtime;
using Microsoft.Scripting.Hosting;

namespace IronDragon {
    /// <summary>
    ///     Main class for users of Dragon.
    /// </summary>
    public static class Dragon {
        private static readonly LanguageSetup _DragonSetup =
            new("IronDragon.Runtime.DragonContext,IronDragon,Version=0.5.0.0,Culture=neutral",
                "IronDragon 0.5", new[] {"IronDragon"}, new[] {".dragon", ".drg"});

        static Dragon() {
            Globals = CreateRuntime().GetEngine("IronDragon").CreateScope();
            new Kernel();
        }

        public static DragonScope CurrentContext { get; internal set; }

        public static ScriptScope Globals { get; }

        /// <summary>
        ///     Generate a FunctionArgument object to be used for named arguments when calling Dragon functions from a .NET
        ///     language.
        /// </summary>
        /// <example>(Where testfunc is a Dragon dynamic function): testfunc(Dragon.Arg("b",2),10);</example>
        /// <param name="name">Name of the argument.</param>
        /// <param name="value">Value for the argument.</param>
        public static FunctionArgument Arg(dynamic name, dynamic value) {
            return new FunctionArgument(name.ToString(), Expression.Constant(value));
        }

        /// <summary>
        ///     Returns a DragonNativeFunction invokable wrapper around the Arg() function that allows you to consume Dragon
        ///     keyword arguments from other languages, pursuant to the limitations of your language.
        /// </summary>
        /// <example>scope.SetVariable("sa",Dragon.GetFunctionArgumentGenerator())</example>
        /// <returns>A DragonNativeFuncton dynamic wrapper around Dragon.Arg(name,value).</returns>
        public static dynamic GetFunctionArgumentGenerator() {
            return new DragonNativeFunction(typeof (Dragon),
                typeof (Dragon).GetMethod("Arg", BindingFlags.Public | BindingFlags.Static));
        }

        public static dynamic Box(object obj, DragonScope scope = null) {
            return DragonBoxedInstance.Box(obj, scope ?? new DragonScope());
        }

        public static dynamic BoxNoCache(object obj, DragonScope scope = null) {
            return DragonBoxedInstance.BoxNoCache(obj, scope ?? new DragonScope());
        }

        public static ScriptRuntime CreateRuntime(params LanguageSetup[] setups) {
            var setup = new ScriptRuntimeSetup();
            setups.ToList().ForEach(lsetup => setup.LanguageSetups.Add(lsetup));
            return new ScriptRuntime(setup);
        }

        public static ScriptRuntime CreateRuntime() {
            return CreateRuntime(_DragonSetup);
        }

        public static LanguageSetup CreateDragonSetup() {
            return _DragonSetup;
        }

        public static dynamic Execute(string source, ScriptScope scope) {
            return CreateRuntime().GetEngine("IronDragon").CreateScriptSourceFromString(source).Execute(scope);
        }

        public static dynamic Execute(string source) {
            return Execute(source, Globals);
        }

        public static dynamic Box(Type type) {
            return DragonClass.BoxClass(type);
        }
    }

    public static class DragonExtensions {
        public static dynamic Eval(this string source) {
            return Dragon.Execute(source);
        }
    }
}