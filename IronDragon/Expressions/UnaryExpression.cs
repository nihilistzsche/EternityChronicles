// UnaryExpression.cs in EternityChronicles/IronDragon
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
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     Represents a unary operator expression in Dragon.
    /// </summary>
    public class UnaryExpression : DragonExpression
    {
        internal UnaryExpression(Expression expr, ExpressionType type)
        {
            Expression = expr;
            UnaryNodeType = type;
        }

        public Expression Expression { get; }

        public ExpressionType UnaryNodeType { get; }

        public override Type Type => UnaryNodeType == ExpressionType.Not ? typeof(bool) : typeof(object);

        private static Expression Convert<T>(Expression e)
        {
            return Convert(e, typeof(T));
        }

        public override Expression Reduce()
        {
            return Operation.Unary(Type, Convert<object>(Expression), Constant(UnaryNodeType), Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Expression.SetScope(scope);
        }

        public override string ToString()
        {
            switch (UnaryNodeType)
            {
            case ExpressionType.OnesComplement:
                return string.Format("~{0}", Expression);
            case ExpressionType.Not:
                return string.Format("!{0}", Expression);
            default:
                return MakeUnary(UnaryNodeType, Expression, Type).ToString();
            }
        }
    }
}