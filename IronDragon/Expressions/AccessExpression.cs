// AccessExpression.cs in EternityChronicles/IronDragon
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
using System.Text;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     Represents an index style access, until [] and []= method calls are implemented.
    /// </summary>
    public class AccessExpression : DragonExpression
    {
        internal AccessExpression(Expression container, List<FunctionArgument> arguments)
        {
            Container = container;
            Arguments = arguments;
        }

        /// <summary>
        ///     Gets the container used in this expression.
        /// </summary>
        /// <value>The container.</value>
        public Expression Container { get; }

        /// <summary>
        ///     Gets the arguments used for this expression.
        /// </summary>
        /// <value>The arguments.</value>
        public List<FunctionArgument> Arguments { get; }

        /// <summary>
        ///     The type returned by this expression.
        /// </summary>
        /// <value>The type.</value>
        public override Type Type => typeof(object);

        /// <summary>
        ///     Reduces this expression into a dynamic expression using <c><see cref="Dragon.Runtime.Operation" />.Access</c>
        /// </summary>
        /// <returns>The reduced expression</returns>
        public override Expression Reduce()
        {
            return Operation.Access(typeof(object), Container, Constant(Arguments), Constant(Scope));
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current
        ///     <see cref="IronDragon.Expressions.AccessExpression" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents the current
        ///     <see cref="IronDragon.Expressions.AccessExpression" />.
        /// </returns>
        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendFormat("{0}[", Container);
            Arguments.ForEach(arg => str.AppendFormat("{0},", arg));

            str.Remove(str.Length - 1, 1);
            str.Append("]");
            return str.ToString();
        }

        /// <summary>
        ///     Called by SetScope to set the children scope on expressions. Should be overridden to tell the runtime which
        ///     children should have scopes set.
        /// </summary>
        /// <param name="scope"></param>
        public override void SetChildrenScopes(DragonScope scope)
        {
            Container.SetScope(scope);
            foreach (var arg in Arguments) arg.Value.SetScope(scope);
        }
    }
}