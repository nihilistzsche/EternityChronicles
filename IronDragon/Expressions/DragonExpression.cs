// DragonExpression.cs
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