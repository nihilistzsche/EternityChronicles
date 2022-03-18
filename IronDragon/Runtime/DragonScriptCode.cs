// -----------------------------------------------------------------------
// <copyright file="DragonScriptCode.cs" company="Michael Tindal">
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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using IronDragon.Builtins;
using IronDragon.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using BlockExpression = IronDragon.Expressions.BlockExpression;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class DragonScriptCode : ScriptCode
    {
        public DragonScriptCode(Expression body, SourceUnit sourceUnit) : base(sourceUnit)
        {
            Body = body;
        }

        /// <summary>
        ///     Returns the body associated with this script code.
        /// </summary>
        public Expression Body { get; }

        private static dynamic ConvertElements(DragonArray res)
        {
            for (var i = 0; i < res.Count(); i++)
            {
                if (res[i] is DragonString) res[i]     = (string)res[i];
                if (res[i] is DragonNumber) res[i]     = DragonNumber.Convert(res[i]);
                if (res[i] is DragonArray) res[i]      = ConvertElements((DragonArray)res[i]);
                if (res[i] is DragonDictionary) res[i] = ConvertElements((DragonDictionary)res[i]);
            }

            return res;
        }

        private static dynamic ConvertElements(DragonDictionary res)
        {
            List<dynamic> keysToRemove = new List<object>();
            keysToRemove.AddRange(res.Keys.OfType<DragonString>());
            keysToRemove.ForEach(o =>
            {
                string s   = o;
                var    val = res[o];
                res.Remove(o);
                res[s] = val;
            });

            keysToRemove.Clear();

            keysToRemove.AddRange(
            res.Keys.Where(
            key =>
                res[key] is DragonString || res[key] is DragonNumber || res[key] is DragonArray ||
                res[key] is DragonDictionary));

            keysToRemove.ForEach(o =>
            {
                if (res[o] is DragonString)
                {
                    string s = res[o];
                    res[o] = s;
                }
                else if (res[o] is DragonNumber)
                {
                    res[o] = DragonNumber.Convert(res[o]);
                }
                else if (res[o] is DragonArray)
                {
                    res[o] = ConvertElements((DragonArray)res[o]);
                }
                else if (res[o] is DragonDictionary)
                {
                    res[o] = ConvertElements((DragonDictionary)res[o]);
                }
            });

            return res;
        }

        internal static dynamic Convert(dynamic res, DragonScope scope)
        {
            if (res is Symbol)
            {
                var symval = new BlockExpression(new List<Expression> { new VariableExpression(res) }, scope);
                res = CompilerServices.CreateLambdaForExpression(symval)();
            }
            else if (res is DragonInstance)
            {
                var so                             = (DragonInstance)res;
                if (so is DragonBoxedInstance) res = ((DragonBoxedInstance)so).BoxedObject;
            }

            if (res is DragonNumber)
                res = DragonNumber.Convert(res);
            else if (res is DragonString)
                res = (string)res;
            else if (res is DragonArray)
                res                               = ConvertElements((DragonArray)res);
            else if (res is DragonDictionary) res = ConvertElements((DragonDictionary)res);

            return res;
        }

        public override object Run(Scope scope)
        {
            var body = Body as BlockExpression;
            body.Scope.MergeWithScope(scope);

            var visitor = new VariableNameVisitor();
            visitor.Visit(body);

            body.SetChildrenScopes(body.Scope);

            var block = CompilerServices.CreateLambdaForExpression(body);
            var res   = block();

            res = Convert(res, body.Scope);

            body.Scope.MergeIntoScope(scope);

            return res;
        }
    }
}