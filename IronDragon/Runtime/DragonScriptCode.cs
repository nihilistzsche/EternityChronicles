// DragonScriptCode.cs in EternityChronicles/IronDragon
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
                if (res[i] is DragonString) res[i] = (string)res[i];
                if (res[i] is DragonNumber) res[i] = DragonNumber.Convert(res[i]);
                if (res[i] is DragonArray) res[i] = ConvertElements((DragonArray)res[i]);
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
                                     string s = o;
                                     var val = res[o];
                                     res.Remove(o);
                                     res[s] = val;
                                 });

            keysToRemove.Clear();

            keysToRemove.AddRange(
                                  res.Keys.Where(
                                                 key =>
                                                     res[key] is DragonString || res[key] is DragonNumber ||
                                                     res[key] is DragonArray ||
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
                var so = (DragonInstance)res;
                if (so is DragonBoxedInstance) res = ((DragonBoxedInstance)so).BoxedObject;
            }

            if (res is DragonNumber)
                res = DragonNumber.Convert(res);
            else if (res is DragonString)
                res = (string)res;
            else if (res is DragonArray)
                res = ConvertElements((DragonArray)res);
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
            var res = block();

            res = Convert(res, body.Scope);

            body.Scope.MergeIntoScope(scope);

            return res;
        }
    }
}