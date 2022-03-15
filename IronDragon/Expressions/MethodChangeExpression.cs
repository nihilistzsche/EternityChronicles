// -----------------------------------------------------------------------
// <copyright file="MethodChangeExpression.cs" Company="Michael Tindal">
// Copyright 2011-2014 Michael Tindal
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
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class MethodChangeExpression : DragonExpression
    {
        internal MethodChangeExpression(string varName, bool isRemove)
        {
            VarName  = varName;
            IsRemove = isRemove;
        }

        public bool IsRemove { get; }

        public string VarName { get; }

        public override Type Type => typeof(object);

        // Should not actually reduce, used by the runtime directly
        public override Expression Reduce()
        {
            return Operation.MethodChange(Type, Constant(VarName), Constant(IsRemove));
        }

        public override string ToString()
        {
            return "";
        }
    }
}