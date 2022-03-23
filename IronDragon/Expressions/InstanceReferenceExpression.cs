// InstanceReferenceExpression.cs in EternityChronicles/IronDragon
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
    public class InstanceReferenceExpression : VariableExpression
    {
        public InstanceReferenceExpression(Expression lvalue, Expression key) : base(Constant(""))
        {
            LValue = lvalue;
            Key = key;
        }

        public Expression LValue { get; }

        public Expression Key { get; }

        public override Type Type => typeof(InstanceReference);

        public override string ToString()
        {
            return string.Format("{0}.{1}", LValue, Key);
        }

        public override Expression Reduce()
        {
            return Operation.InstanceRef(typeof(InstanceReference), Constant(LValue), Key);
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            LValue.SetScope(scope);
            Key.SetScope(scope);
        }
    }
}