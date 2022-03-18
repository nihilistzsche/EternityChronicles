// -----------------------------------------------------------------------
// <copyright file="ConvertExpression.cs" Company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

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