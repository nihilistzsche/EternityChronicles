// ReturnExpression.cs in EternityChronicles/IronDragon
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
using IronDragon.Parser;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ReturnExpression : DragonExpression
    {
        internal ReturnExpression(List<FunctionArgument> arguments)
        {
            Arguments = arguments;
        }

        public List<FunctionArgument> Arguments { get; }

        public override Type Type => typeof(object);

        public override Expression Reduce()
        {
            var realArgs = new List<Expression>();
            Arguments.ForEach(arg => realArgs.Add(arg.Value));

            Expression returnVal;
            if (realArgs.Count > 1)
                returnVal = CreateArray(realArgs);
            else
                returnVal = realArgs.Count == 0 ? Constant(null) : realArgs[0];

            return Return(DragonParser.ReturnTarget, Convert(returnVal, typeof(object)), typeof(object));
        }

        public override string ToString()
        {
            var str = new StringBuilder("return ");
            Arguments.ForEach(arg => str.AppendFormat("{0},", arg));

            str.Remove(str.Length - 1, 1);
            return str.ToString();
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            foreach (var arg in Arguments) arg.Value.SetScope(scope);
        }
    }
}