// ParallelAssignmentExpression.cs in EternityChronicles/IronDragon
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
    using CS = CompilerServices;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ParallelAssignmentExpression : DragonExpression
    {
        internal ParallelAssignmentExpression(List<ParallelAssignmentInfo> lvalues,
                                              List<ParallelAssignmentInfo> rvalues)
        {
            LeftHandValues = lvalues;
            RightHandValues = rvalues;
        }

        public List<ParallelAssignmentInfo> LeftHandValues { get; }
        public List<ParallelAssignmentInfo> RightHandValues { get; }

        public override Type Type => typeof(object);

        public override Expression Reduce()
        {
            return Operation.ParallelAssign(typeof(object), Constant(LeftHandValues), Constant(RightHandValues),
                                            Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            LeftHandValues.ForEach(value =>
                                   {
                                       if (value.Value is DragonExpression) value.Value.SetScope(scope);
                                   });
            RightHandValues.ForEach(value =>
                                    {
                                        if (value.Value is DragonExpression) value.Value.SetScope(scope);
                                    });
        }

        public override string ToString()
        {
            return "";
        }

        /// <summary>
        ///     Used to represent lvalues and rvalues in a parallel assignment statement.
        /// </summary>
        public struct ParallelAssignmentInfo
        {
            public bool IsWildcard;
            public dynamic Value;
        }
    }
}