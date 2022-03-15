// -----------------------------------------------------------------------
// <copyright file="AccessSetExpression.cs" Company="Michael Tindal">
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

using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions {
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class AccessSetExpression : AccessExpression {
        internal AccessSetExpression(Expression container, List<FunctionArgument> arguments, Expression value)
            : this(container, arguments, value, ExpressionType.Assign) {}

        internal AccessSetExpression(Expression container, List<FunctionArgument> arguments, Expression value,
            ExpressionType extra)
            : base(container, arguments) {
            Value = value;
            ExtraNodeType = extra;
        }

        public Expression Value { get; }

        public ExpressionType ExtraNodeType { get; }

        public override Expression Reduce() {
            return Operation.AccessSet(Type, Container, Constant(Arguments), Convert(Value, typeof (object)),
                Constant(ExtraNodeType), Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope) {
            base.SetChildrenScopes(scope);
            Value.SetScope(scope);
        }
    }
}