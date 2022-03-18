using System;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    public class RequireExpression : DragonExpression
    {
        public RequireExpression(Expression value)
        {
            Value = value;
        }

        public Expression Value { get; }

        public override Type Type => typeof(object);

        public override Expression Reduce()
        {
            return Operation.Require(Type, Value, Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Value.SetScope(scope);
        }

        public override string ToString()
        {
            return $"require(\"#{Value}\")";
        }
    }
}