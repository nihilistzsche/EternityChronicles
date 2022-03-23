// SyncExpression.cs in EternityChronicles/IronDragon
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
    public class SyncExpression : DragonExpression
    {
        internal SyncExpression(string varName, Expression body)
        {
            Body = body;
            VarName = varName;
        }

        public Expression Body { get; }

        public string VarName { get; }

        public override Type Type => Body.Type;

        // Should not actually reduce, used by the runtime directly
        public override Expression Reduce()
        {
            return Operation.Sync(Type, Constant(VarName), Constant(Body), Constant(Scope));
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