// -----------------------------------------------------------------------
// <copyright file="SingletonDefinitionExpression.cs" Company="Michael Tindal">
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
using System.Linq.Expressions;
using IronDragon.Parser;
using IronDragon.Runtime;

namespace IronDragon.Expressions {
    public class SingletonDefinitionExpression : FunctionDefinitionExpression {
        public SingletonDefinitionExpression(Expression singleton, string name, List<FunctionArgument> arguments,
            Expression body) : base(name, arguments, body) {
            Singleton = singleton;
        }

        public Expression Singleton { get; }

        public override Expression Reduce() {
            var ci = 0;
            Arguments.ForEach(arg => arg.Index = ci++);
            var realBody = new List<Expression>(((BlockExpression) Body).Body);
            if (Name == "new") {
                realBody.Add(Return(new List<FunctionArgument> {new(null, Variable(Constant("self")))}));
            }
            realBody.Add(Label(DragonParser.ReturnTarget, Constant(null, typeof (object))));

            return Operation.SingletonDefine(typeof (DragonFunction), Constant(Singleton), Constant(Name),
                Constant(Arguments),
                Constant(DragonParser.CreateBlock(realBody)),
                Constant(Scope));
        }
    }
}