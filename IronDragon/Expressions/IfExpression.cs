// -----------------------------------------------------------------------
// <copyright file="IfExpression.cs" Company="Michael Tindal">
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
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class IfExpression : DragonExpression
    {
        internal IfExpression(Expression test, Expression ifTrue, Expression ifFalse)
        {
            Test    = test;
            IfTrue  = ifTrue;
            IfFalse = ifFalse;
        }

        public Expression Test { get; }

        public Expression IfTrue { get; }

        public Expression IfFalse { get; }

        public override Type Type => IfTrue.Type;

        public override Expression Reduce()
        {
            var rt = IfTrue;
            var rf = IfFalse;
            if (rf != null)
                if (rf.Type != rt.Type)
                    rf = Convert(rf, rt.Type);
            return Condition(Boolean(Test), rt, rf ?? Default(rt.Type), Type);
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Test.SetScope(scope);
            IfTrue.SetScope(scope);
            IfFalse.SetScope(scope);
        }

        public override string ToString()
        {
            return "";
        }
    }
}