// -----------------------------------------------------------------------
// <copyright file="AssignmentExpression.cs" Company="Michael Tindal">
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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;
using IronDragon.Runtime;

namespace IronDragon.Expressions {
    using CS = CompilerServices;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class AssignmentExpression : DragonExpression {
        internal AssignmentExpression(LeftHandValueExpression left, Expression right, ExpressionType assignType) {
            Left = left;
            Right = right ?? Constant(null);
            ExtraNodeType = assignType;
        }

        public LeftHandValueExpression Left { get; }

        public Expression Right { get; }

        internal bool IsConst { get; set; }

        public ExpressionType ExtraNodeType { get; }

        public override Type Type => Right.Type;

        public override void SetChildrenScopes(DragonScope scope) {
            Left.SetScope(scope);
            Right.SetScope(scope);
        }

        public override Expression Reduce() {
            var rl = Left.Reduce();

            if (rl is VariableExpression) {
                return Operation.Assign(Right.Type, Constant(rl), Convert(Right, typeof (object)),
                    Constant(ExtraNodeType), Constant(IsConst), Constant(Scope));
            }
            if (rl is AccessExpression) {
                return
                    Convert(
                        AccessSet((rl as AccessExpression).Container, (rl as AccessExpression).Arguments,
                            Right, ExtraNodeType), Type);
            }
            return Right;
        }

        public override string ToString() {
            var assign = new Dictionary<ExpressionType, string>();
            assign[ExpressionType.Assign] = "=";
            assign[ExpressionType.AddAssign] = "+=";
            assign[ExpressionType.SubtractAssign] = "-=";
            assign[ExpressionType.MultiplyAssign] = "*=";
            assign[ExpressionType.DivideAssign] = "/=";
            assign[ExpressionType.ModuloAssign] = "%=";
            assign[ExpressionType.LeftShiftAssign] = "<<=";
            assign[ExpressionType.RightShiftAssign] = ">>=";
            assign[ExpressionType.AndAssign] = "&=";
            assign[ExpressionType.OrAssign] = "|=";
            assign[ExpressionType.ExclusiveOrAssign] = "^=";
            assign[ExpressionType.PowerAssign] = "**=";
            return string.Format("{0} {1} {2}", Left, assign[ExtraNodeType], Right);
        }
    }
}