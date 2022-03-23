// FunctionCallExpression.cs in EternityChronicles/IronDragon
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
    public class FunctionCallExpression : DragonExpression
    {
        internal FunctionCallExpression(Expression func, List<FunctionArgument> arguments,
                                        DragonExpressionType pipeType)
        {
            Function = func;
            Arguments = arguments;
            PipeType = pipeType;
        }

        internal FunctionCallExpression(Expression func, List<FunctionArgument> arguments)
            : this(func, arguments, DragonExpressionType.Empty)
        {
        }

        internal FunctionCallExpression(Expression func, List<FunctionArgument> arguments, bool isUnaryOp,
                                        bool isPostfix) : this(func, arguments)
        {
            IsOp = isUnaryOp;
            IsPostfix = isPostfix;
        }

        public DragonExpressionType PipeType { get; }

        public Expression Function { get; }

        public bool IsPostfix { get; }

        public bool IsOp { get; }

        public List<FunctionArgument> Arguments { get; }

        public override Type Type => typeof(object);

        public override Expression Reduce()
        {
            if (Function is VariableExpression && ((VariableExpression)Function).Name is InstanceReferenceExpression)
                return Operation.Call(typeof(object), ((VariableExpression)Function).Name, Constant(Arguments),
                                      Constant(Scope), Constant(PipeType), Constant(IsOp), Constant(IsPostfix));
            return Operation.Call(typeof(object), Function, Constant(Arguments), Constant(Scope), Constant(PipeType),
                                  Constant(IsOp), Constant(IsPostfix));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Function.SetScope(scope);
        }

        public override string ToString()
        {
            return string.Format("[FunctionCallExpression Function: {0} Arguments: {1}", Function, Arguments);
        }
    }
}