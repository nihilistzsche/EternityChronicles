// AssignmentExpression.cs in EternityChronicles/IronDragon
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
using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    using CS = CompilerServices;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class AssignmentExpression : DragonExpression
    {
        internal AssignmentExpression(LeftHandValueExpression left, Expression right, ExpressionType assignType)
        {
            Left = left;
            Right = right ?? Constant(null);
            ExtraNodeType = assignType;
        }

        public LeftHandValueExpression Left { get; }

        public Expression Right { get; }

        internal bool IsConst { get; set; }

        public ExpressionType ExtraNodeType { get; }

        public override Type Type => Right.Type;

        public override void SetChildrenScopes(DragonScope scope)
        {
            Left.SetScope(scope);
            Right.SetScope(scope);
        }

        public override Expression Reduce()
        {
            var rl = Left.Reduce();


            if (rl is AccessExpression)
                return
                    Convert(
                            AccessSet((rl as AccessExpression).Container, (rl as AccessExpression).Arguments,
                                      Right, ExtraNodeType), Type);
            return Operation.Assign(Right.Type, Constant(rl), Convert(Right, typeof(object)),
                                    Constant(ExtraNodeType), Constant(IsConst), Constant(Scope));
        }

        public override string ToString()
        {
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