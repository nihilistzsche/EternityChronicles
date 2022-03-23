// SetAssignExpression.cs in EternityChronicles/IronDragon
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
    public class SetAssignExpression : DragonExpression
    {
        internal SetAssignExpression(Expression left, Expression right, ExpressionType type)
        {
            Left = left;
            Right = right;
            SetNodeType = type;
        }

        /// <summary>
        ///     The left side of the comparison.
        /// </summary>
        public Expression Left { get; private set; }

        /// <summary>
        ///     The right side of the comparison.
        /// </summary>
        public Expression Right { get; private set; }

        protected ExpressionType SetNodeType { get; }

        public override Type Type => Right.Type;

        /// <summary>
        ///     Reduces this expression to base DLR variables.
        /// </summary>
        /// <returns></returns>
        public override Expression Reduce()
        {
            var expr = new AssignmentExpression(Left as LeftHandValueExpression, Right, SetNodeType);

            expr.SetScope(Scope.ParentScope);
            expr.Right.SetScope(Scope);

            return expr;
        }

        /// <summary>
        ///     Converts this expression to a string representing its contents.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return "set " + new AssignmentExpression(Left as LeftHandValueExpression, Right, SetNodeType);
        }

        /// <summary>
        ///     Visits the children of this expression.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns>A new expression with the modified children.</returns>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Left = visitor.Visit(Left);
            Right = visitor.Visit(Right);
            return new SetAssignExpression(Left, Right, SetNodeType);
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Left.SetScope(scope);
            Right.SetScope(scope);
        }
    }
}