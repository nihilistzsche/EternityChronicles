// WhileExpression.cs
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
    ///     Represents a while expression for Dragon.
    /// </summary>
    public class WhileExpression : DragonExpression
    {
        internal WhileExpression(Expression test, Expression body)
        {
            Test = test;
            Body = body;
        }

        public Expression Test { get; }

        public Expression Body { get; }

        public override Type Type => Body.Type;

        public override Expression Reduce()
        {
            var whileLabel = Label("<dragon_while>");
            ParameterExpression whileReturn = null;
            var useReturn = true;
            if (Body.Type == typeof(void))
                useReturn = false;
            else
                whileReturn = Variable(Body.Type, "<dragon_while_return>");
            var whileTest = Variable(typeof(bool), "<dragon_while_test>");
            var realBody = new List<Expression>
                           {
                               Label(whileLabel),
                               Label(DragonParser.ContinueTarget),
                               Assign(whileTest, Boolean(Test)),
                               Label(DragonParser.RetryTarget),
                               useReturn
                                   ? IfThen(whileTest, Assign(whileReturn, Body))
                                   : IfThen(whileTest, Body),
                               IfThen(whileTest, Goto(whileLabel)),
                               Label(DragonParser.BreakTarget)
                           };
            if (useReturn)
            {
                realBody.Add(Convert(whileReturn, Body.Type));

                return Block(new[] { whileTest, whileReturn }, realBody);
            }

            return Block(new[] { whileTest }, realBody);
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Test.SetScope(scope);
            Body.SetScope(scope);
        }

        public override string ToString()
        {
            return "";
        }
    }
}