// -----------------------------------------------------------------------
// <copyright file="CreateDictionaryExpression.cs" Company="Michael Tindal">
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