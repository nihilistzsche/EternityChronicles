// -----------------------------------------------------------------------
// <copyright file="LeftHandValueExpression.cs" Company="Michael Tindal">
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

namespace IronDragon.Expressions {
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class LeftHandValueExpression : DragonExpression {
        internal LeftHandValueExpression(Expression value) {
            Value = value;
        }

        public Expression Value { get; }

        public override Type Type => Value.Type;

        public override Expression Reduce() {
            if (Value is IndexExpression) {
                var i = (IndexExpression) Value;
                var obj = i.Object;
                if (obj is VariableExpression) {
                    return i.Update((obj as VariableExpression).Reduce(), i.Arguments);
                }
                return i;
            }
            return Value;
        }

        public override string ToString() {
            return string.Format("lvalue({0})", Value);
        }

        public override void SetChildrenScopes(DragonScope scope) {
            Value.SetScope(scope);
        }
    }
}