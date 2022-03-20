// FunctionDefinitionExpression.cs in EternityChronicles/IronDragon
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
using IronDragon.Parser;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class FunctionDefinitionExpression : DragonExpression
    {
        internal FunctionDefinitionExpression(string name, List<FunctionArgument> arguments, Expression body)
        {
            Name = name;
            Arguments = arguments;
            Body = body;
        }

        public string Name { get; }

        public List<FunctionArgument> Arguments { get; }

        public Expression Body { get; }

        public override Type Type => typeof(DragonFunction);

        public override Expression Reduce()
        {
            var ci = 0;
            Arguments.ForEach(arg => arg.Index = ci++);
            var realBody = new List<Expression>(((BlockExpression)Body).Body);
            if (Name == "new")
                realBody.Add(Return(new List<FunctionArgument> { new(null, Variable(Constant("self"))) }));
            realBody.Add(Label(DragonParser.ReturnTarget, Constant(null, typeof(object))));

            return Operation.Define(typeof(DragonFunction), Constant(Name),
                                    Constant(Arguments),
                                    Constant(DragonParser.CreateBlock(realBody)),
                                    Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Body.SetScope(scope);
        }

        public override string ToString()
        {
            return string.Format("[FunctionDefinitionExpression: Name={0}, Arguments={1}, Block={2}, Type={3}]", Name,
                                 Arguments, Body, Type);
        }
    }
}