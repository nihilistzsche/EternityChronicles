// LoopExpression.cs in EternityChronicles/IronDragon
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
    public class LoopExpression : DragonExpression
    {
        internal LoopExpression(Expression body)
        {
            Body = body;
        }

        public Expression Body { get; }

        public override Type Type => Body.Type;

        public override Expression Reduce()
        {
            var loopLabel = Label("<dragon_loop>");
            ParameterExpression loopReturn = null;
            var useReturn = true;
            if (Body.Type == typeof(void))
                useReturn = false;
            else
                loopReturn = Variable(Body.Type, "<dragon_loop_return>");
            var realBody = new List<Expression>
                           {
                               Label(loopLabel),
                               Label(DragonParser.ContinueTarget),
                               Label(DragonParser.RetryTarget),
                               useReturn
                                   ? Assign(loopReturn, Body)
                                   : Body,
                               Goto(loopLabel),
                               Label(DragonParser.BreakTarget)
                           };
            if (useReturn)
            {
                realBody.Add(Convert(loopReturn, Body.Type));

                return Block(new[] { loopReturn }, realBody);
            }

            return Block(new ParameterExpression[] { }, realBody);
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Body.SetScope(scope);
        }

        public override string ToString()
        {
            return $"loop {{ {Body} }}";
        }
    }
}