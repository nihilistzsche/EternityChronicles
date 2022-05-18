// BinaryExpression.cs in EternityChronicles/IronDragon
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
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class BinaryExpression : DragonExpression
    {
        private readonly bool _isDragonBinary;

        internal BinaryExpression(Expression left, Expression right, ExpressionType type)
        {
            Left = left;
            Right = right;
            BinaryNodeType = type;
            _isDragonBinary = false;
        }

        internal BinaryExpression(Expression left, Expression right, DragonExpressionType type)
        {
            Left = left;
            Right = right;
            DragonBinaryNodeType = type;
            _isDragonBinary = true;
        }

        public ExpressionType BinaryNodeType { get; }

        public DragonExpressionType DragonBinaryNodeType { get; }

        public Expression Left { get; }

        public Expression Right { get; }

        public override Type Type
        {
            get
            {
                if (_isDragonBinary)
                {
                    if (DragonBinaryNodeType == DragonExpressionType.Compare) return typeof(int);
                    return typeof(bool);
                }

                switch (BinaryNodeType)
                {
                case ExpressionType.AndAlso:
                    return typeof(bool);
                case ExpressionType.Equal:
                    return typeof(bool);
                case ExpressionType.GreaterThan:
                    return typeof(bool);
                case ExpressionType.GreaterThanOrEqual:
                    return typeof(bool);
                case ExpressionType.LessThan:
                    return typeof(bool);
                case ExpressionType.LessThanOrEqual:
                    return typeof(bool);
                case ExpressionType.NotEqual:
                    return typeof(bool);
                case ExpressionType.OrElse:
                    return typeof(bool);
                default:
                    return typeof(object);
                }
            }
        }

        private static Expression Convert<T>(Expression e)
        {
            return Convert(e, typeof(T));
        }

        public override Expression Reduce()
        {
            if (_isDragonBinary)
                switch (DragonBinaryNodeType)
                {
                case DragonExpressionType.Compare:
                    return ReduceCompare();
                case DragonExpressionType.LogicalXor:
                    return ReduceLogicalXor();
                case DragonExpressionType.Match:
                    return ReduceMatch();
                case DragonExpressionType.NotMatch:
                    return ReduceMatch();
                }

            if (BinaryNodeType is ExpressionType.OrElse or ExpressionType.AndAlso)
                return MakeBinary(BinaryNodeType, Convert<bool>(Left), Convert<bool>(Right));
            return Operation.Binary(Type, Convert<object>(Left), Convert<object>(Right), Constant(BinaryNodeType),
                                    Constant(Scope));
        }

        protected Expression ReduceCompare()
        {
            return Operation.Compare(typeof(int), Convert(Left, typeof(object)), Convert(Right, typeof(object)),
                                     Constant(Scope));
        }

        protected Expression ReduceLogicalXor()
        {
            return new IfExpression(
                                    new BinaryExpression(Left, Right, ExpressionType.AndAlso),
                                    Constant(false),
                                    new IfExpression(
                                                     new BinaryExpression(Left, Right, ExpressionType.OrElse),
                                                     Constant(true),
                                                     Constant(false)
                                                    )
                                   );
        }

        protected Expression ReduceMatch()
        {
            return Operation.Match(typeof(bool), Constant(Left), Constant(Right), Constant(DragonBinaryNodeType),
                                   Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Left.SetScope(scope);
            Right.SetScope(scope);
        }

        public override string ToString()
        {
            var ops = new Dictionary<ExpressionType, string>();
            ops[ExpressionType.Add] = "+";
            ops[ExpressionType.And] = "&";
            ops[ExpressionType.AndAlso] = "&&";
            ops[ExpressionType.Divide] = "/";
            ops[ExpressionType.Equal] = "==";
            ops[ExpressionType.ExclusiveOr] = "^";
            ops[ExpressionType.GreaterThan] = ">";
            ops[ExpressionType.GreaterThanOrEqual] = ">=";
            ops[ExpressionType.LeftShift] = "<<";
            ops[ExpressionType.LessThan] = "<";
            ops[ExpressionType.LessThanOrEqual] = "<=";
            ops[ExpressionType.Modulo] = "%";
            ops[ExpressionType.Multiply] = "*";
            ops[ExpressionType.NotEqual] = "!=";
            ops[ExpressionType.Or] = "|";
            ops[ExpressionType.OrElse] = "||";
            ops[ExpressionType.Power] = "**";
            ops[ExpressionType.RightShift] = ">>";
            ops[ExpressionType.Subtract] = "-";
            return string.Format("({0} {1} {2})", Left, ops[BinaryNodeType], Right);
        }
    }
}