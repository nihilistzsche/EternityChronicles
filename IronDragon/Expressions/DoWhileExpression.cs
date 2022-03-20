// DoWhileExpression.cs
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

using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Parser;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class DoWhileExpression : WhileExpression
    {
        internal DoWhileExpression(Expression test, Expression body) : base(test, body)
        {
        }

        public override Expression Reduce()
        {
            var whileLabel = Label("<dragon_do_while>");
            ParameterExpression whileReturn = null;
            var useReturn = true;
            if (Body.Type == typeof(void))
                useReturn = false;
            else
                whileReturn = Variable(Body.Type, "<dragon_do_while_return>");
            var whileTest = Variable(typeof(bool), "<dragon_do_while_test>");
            var realBody = new List<Expression>
                           {
                               Label(whileLabel),
                               Label(DragonParser.RetryTarget),
                               useReturn ? Assign(whileReturn, Body) : Body,
                               Label(DragonParser.ContinueTarget),
                               Assign(whileTest, Boolean(Test)),
                               IfThen(whileTest, Goto(whileLabel)),
                               Label(DragonParser.BreakTarget)
                           };

            if (useReturn)
            {
                realBody.Add(whileReturn);

                return Block(new[]
                             {
                                 whileTest,
                                 whileReturn
                             }, realBody);
            }

            return Block(new[] { whileTest }, realBody);
        }
    }
}