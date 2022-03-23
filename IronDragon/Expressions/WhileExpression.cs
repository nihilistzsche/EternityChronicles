// WhileExpression.cs in EternityChronicles/IronDragon
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