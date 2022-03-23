// RescueExpression.cs in EternityChronicles/IronDragon
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
            Body = body;
            VarName = varName;
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