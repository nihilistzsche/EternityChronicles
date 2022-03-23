// InvokeExpression.cs in EternityChronicles/IronDragon
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
    public class InvokeExpression : DragonExpression
    {
        internal InvokeExpression(Expression type, Expression method, List<FunctionArgument> arguments)
        {
            TargetType = type;
            Method = method;
            Arguments = arguments;
        }

        public Expression TargetType { get; }

        public Expression Method { get; }

        public List<FunctionArgument> Arguments { get; }

        public override Type Type => typeof(object);

        public override Expression Reduce()
        {
            var args = new List<FunctionArgument>();
            Arguments.ForEach(arg => args.Add(new FunctionArgument(arg.Name, Variable(Constant(arg.Name)))));
            return Operation.Invoke(typeof(object), TargetType, Method, Constant(args), Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            TargetType.SetScope(scope);
            Method.SetScope(scope);
        }

        public override string ToString()
        {
            return string.Format("[InvokeExpression: TargetType={0}, Method={1}, Arguments={2}, Type={3}]", TargetType,
                                 Method, Arguments, Type);
        }
    }
}