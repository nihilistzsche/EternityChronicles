// PutsExpression.cs in EternityChronicles/IronDragon
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
using System.Linq.Expressions;
using System.Reflection;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class PutsExpression : DragonExpression
    {
        private static readonly MethodInfo Method = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });

        internal PutsExpression(Expression value)
        {
            Value = value;
        }

        public Expression Value { get; }

        public override Type Type => Method.ReturnType;

        public override Expression Reduce()
        {
            return Call(null, Method, Call(Value, typeof(object).GetMethod("ToString")));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Value.SetScope(scope);
        }

        public override string ToString()
        {
            return string.Format("puts {0}", Value);
        }
    }
}