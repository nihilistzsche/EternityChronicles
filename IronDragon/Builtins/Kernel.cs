// Kernel.cs in EternityChronicles/IronDragon
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

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using IronDragon.Expressions;
using IronDragon.Parser;
using IronDragon.Runtime;
using BlockExpression = IronDragon.Expressions.BlockExpression;

namespace IronDragon.Builtins
{
    /// <summary>
    ///     The Kernel class provides the runtime methods every Dragon instance needs.  It corresponds to core.sv in default
    ///     Silver runtime.
    /// </summary>
    [DragonDoNotExport]
    public class Kernel
    {
        public Kernel()
        {
            DragonNativeFunction nfunc;

            GetType().GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList()
                     .ForEach(method =>
                              {
                                  nfunc = new DragonNativeFunction(GetType(), method);
                                  Dragon.Globals[nfunc.Name] = nfunc;
                              });
            var assembly = typeof(Kernel).Assembly;
            var resourceName = assembly.GetManifestResourceNames().Single(n => n.EndsWith("core.dragon"));

            var stream = assembly.GetManifestResourceStream(resourceName);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            var source = Encoding.UTF8.GetString(buffer);

            Dragon.Execute(source);
        }

        [DragonExport("_eval")]
        private static dynamic Eval(string eval, DragonScope scope)
        {
            var xexpression = string.Format("{0};", eval);

            var res = DragonParser.Parse(xexpression);
            Expression block;
            if (res != null)
            {
                block = DragonExpression.DragonBlock(res);
                // We want eval'd expressions to execute in the current scope, not its own child scopes.  This ensures assignment evals work properly.
                ((BlockExpression)block).Scope = scope;
                ((BlockExpression)block).SetChildrenScopes(scope);
            }
            else
            {
                return null;
            }

            var val = CompilerServices.CreateLambdaForExpression(block)();
            return val;
        }

        [DragonExport("_class_eval")]
        private static dynamic ClassEval(object self, string eval, DragonScope scope)
        {
            DragonClass @class;
            var instance = self as DragonInstance;
            if (instance != null)
                @class = instance.Class;
            else
                @class = self as DragonClass;
            if (@class == null)
                return null;

            var xexpression = string.Format("{0};", eval);

            var res = DragonParser.Parse(xexpression);

            return res != null ? RuntimeOperations.DefineCategory(@class, res.ToList(), scope) : null;
        }

        [DragonExport("_instance_eval")]
        private static dynamic InstanceEval(object self, string eval, DragonScope scope)
        {
            if (!(self is DragonInstance instance)) return null;

            var xexpression = string.Format("{0};", eval);
            var res = DragonParser.Parse(xexpression);
            Expression block;
            if (res != null)
            {
                scope["self"] = scope["super"] = instance;
                scope["<dragon_context_invokemember>"] = true;
                string selfName;
                var selfScope = scope.SearchForObject(instance, out selfName);
                if (selfScope != null && selfName != null)
                {
                    scope["<dragon_context_selfscope>"] = selfScope;
                    scope["<dragon_context_selfname>"] = selfName;
                }

                block = DragonExpression.DragonBlock(res);
                // We want eval'd expressions to execute in the current scope, not its own child scopes.  This ensures assignment evals work properly.
                ((BlockExpression)block).Scope = scope;
                ((BlockExpression)block).SetChildrenScopes(scope);
            }
            else
            {
                return null;
            }

            var val = CompilerServices.CreateLambdaForExpression(block)();
            return val;
        }

        [DragonExport("box")]
        private static dynamic Box(object val)
        {
            return Dragon.Box(val);
        }
    }
}