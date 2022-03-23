// IfExpression.cs in EternityChronicles/IronDragon
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
            Test = test;
            IfTrue = ifTrue;
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