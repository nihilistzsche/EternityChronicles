// ThrowExpression.cs in EternityChronicles/IronDragon
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ThrowExpression : DragonExpression
    {
        internal ThrowExpression(Expression exceptionObject)
        {
            ExceptionObject = exceptionObject;
        }

        public Expression ExceptionObject { get; }

        public override Type Type => typeof(object);

        public override void SetChildrenScopes(DragonScope scope)
        {
            ExceptionObject.SetScope(scope);
        }

        public override string ToString()
        {
            return "";
        }

        /// <summary>
        ///     Reduces this expression to base DLR variables.
        /// </summary>
        /// <returns></returns>
        public override Expression Reduce()
        {
            return Operation.Throw(Type, Constant(ExceptionObject), Constant(Scope));
        }
    }
}