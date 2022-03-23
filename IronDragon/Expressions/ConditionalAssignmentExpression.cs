// ConditionalAssignmentExpression.cs in EternityChronicles/IronDragon
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

using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    using CS = CompilerServices;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ConditionalAssignmentExpression : AssignmentExpression
    {
        internal ConditionalAssignmentExpression(LeftHandValueExpression left, Expression right,
                                                 DragonExpressionType conditionalAssignmentType)
            : base(left, right, ExpressionType.Assign)
        {
            ConditionalAssignmentType = conditionalAssignmentType;
        }

        public DragonExpressionType ConditionalAssignmentType { get; }

        public override Expression Reduce()
        {
            var rl = Left.Reduce();
            if (rl is VariableExpression)
                return Operation.ConditionalAssign(Right.Type, Constant(rl), Convert(Right, typeof(object)),
                                                   Constant(ConditionalAssignmentType), Constant(IsConst),
                                                   Constant(Scope));
            return
                Convert(
                        ConditionalAccessSet((rl as AccessExpression).Container,
                                             (rl as AccessExpression).Arguments, Right, ConditionalAssignmentType),
                        Type);
        }

        public override string ToString()
        {
            var opStr = "||=";
            if (ConditionalAssignmentType == DragonExpressionType.IfNotNullAssign) opStr = "&&=";
            return string.Format("{0} {1} {2}", Left, opStr, Right);
        }
    }
}