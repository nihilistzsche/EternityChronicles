// DragonExpression.cs in EternityChronicles/IronDragon
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
    ///     Base class for Dragon abstract syntax tree expressions.
    /// </summary>
    public abstract partial class DragonExpression : Expression, IScopeExpression
    {
        /// <summary>
        ///     All Dragon expressions are extension expressions, so they always return true.
        /// </summary>
        public sealed override bool CanReduce => true;

        /// <summary>
        ///     Returns ExpressionType.Extension for all Dragon expressions.
        /// </summary>
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Should return the type returned by this expression.
        /// </summary>
        public abstract override Type Type { get; }

        /// <summary>
        ///     Sets the scope used by this expression.
        /// </summary>
        /// <param name="scope">Scope for this expression</param>
        public void SetScope(DragonScope scope)
        {
            Scope = scope;
            SetChildrenScopes(scope);
        }

        /// <summary>
        ///     Returns the scope used by this expression.
        /// </summary>
        public DragonScope Scope { get; internal set; }

        /// <summary>
        ///     This method should reduce the Dragon expression into either more Dragon expressions or base DLR
        ///     expressions.  The end result should be an expression tree of base DLR expressions.
        /// </summary>
        /// <returns>The reduced expression</returns>
        public abstract override Expression Reduce();

        /// <summary>
        ///     Called by SetScope to set the children scope on expressions.  Should be overridden to tell the runtime which
        ///     children should have scopes set.
        /// </summary>
        /// <param name="scope"></param>
        public virtual void SetChildrenScopes(DragonScope scope)
        {
        }

        public abstract override string ToString();
    }
}