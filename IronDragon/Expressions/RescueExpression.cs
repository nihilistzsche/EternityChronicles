// -----------------------------------------------------------------------
// <copyright file="RescueExpression.cs" Company="Michael Tindal">
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
using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class RescueExpression : DragonExpression
    {
        internal RescueExpression(List<string> exceptionTypes, Expression body, string varName = "$#")
        {
            ExceptionTypes = exceptionTypes;
            Body           = body;
            VarName        = varName;
        }

        public List<string> ExceptionTypes { get; }

        public Expression Body { get; }

        public string VarName { get; }

        public bool IsWildcard { get; set; }

        public override Type Type => typeof(object);

        // Should not actually reduce, used by the runtime directly
        public override Expression Reduce()
        {
            return Constant(null, typeof(object));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Body.SetScope(scope);
        }

        public override string ToString()
        {
            return "";
        }
    }
}