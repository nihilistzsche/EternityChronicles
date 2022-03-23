// ConvertExpression.cs in EternityChronicles/IronDragon
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
    ///     TODO: Update summary.
    /// </summary>
    public class ConvertExpression : DragonExpression
    {
        internal ConvertExpression(Expression expr, Type type)
        {
            Expression = expr;
            ConvertType = type;
        }

        public Expression Expression { get; }

        public Type ConvertType { get; }

        public override Type Type => ConvertType;

        public override Expression Reduce()
        {
            if (Expression == null)
                return Constant(null);

            if (Expression.Type == ConvertType) return Expression;

            return Operation.Convert(ConvertType, Expression.Convert(Expression, typeof(object)),
                                     Constant(ConvertType));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Expression.SetScope(scope);
        }

        public override string ToString()
        {
            return string.Format("({0} :> {1})", Expression, ConvertType);
        }
    }
}