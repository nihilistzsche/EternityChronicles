// CreateDictionaryExpression.cs in EternityChronicles/IronDragon
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
using IronDragon.Builtins;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class CreateDictionaryExpression : DragonExpression
    {
        internal CreateDictionaryExpression(List<Expression> values)
        {
            Values = values;
        }

        public List<Expression> Values { get; }

        public override Type Type => typeof(DragonDictionary);

        public override Expression Reduce()
        {
            return Operation.CreateDictionary(typeof(DragonDictionary), Constant(Values));
        }

        public override string ToString()
        {
            var str = new StringBuilder("[");
            foreach (var e in Values) str.AppendFormat("{0},", e);
            str.Remove(str.Length - 1, 1);
            str.Append("]");
            return str.ToString();
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            foreach (var value in Values) value.SetScope(scope);
        }
    }
}