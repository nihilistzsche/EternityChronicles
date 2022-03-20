// YieldExpression.cs in EternityChronicles/IronDragon
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class YieldExpression : DragonExpression
    {
        internal YieldExpression(List<FunctionArgument> arguments)
        {
            Arguments = arguments;
        }

        public List<FunctionArgument> Arguments { get; }

        public override Type Type => typeof(object);

        public override Expression Reduce()
        {
            var yVar = Variable(Constant("__yieldBlock"));
            yVar.SetScope(Scope);
            return Operation.Yield(typeof(object), yVar, Constant(Arguments), Constant(Scope));
        }

        public override string ToString()
        {
            var str = new StringBuilder("yield ");
            Arguments.ForEach(arg => str.AppendFormat("{0},", arg));

            str.Remove(str.Length - 1, 1);
            return str.ToString();
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            foreach (var arg in Arguments) arg.Value.SetScope(scope);
        }
    }
}