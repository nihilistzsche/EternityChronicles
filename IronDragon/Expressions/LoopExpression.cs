// LoopExpression.cs in EternityChronicles/IronDragon
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