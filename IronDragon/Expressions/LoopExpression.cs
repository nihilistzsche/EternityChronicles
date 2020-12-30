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
            {
                useReturn = false;
            }
            else
            {
                loopReturn = Variable(Body.Type, "<dragon_loop_return>");
            }
            var realBody = new List<Expression> {
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
