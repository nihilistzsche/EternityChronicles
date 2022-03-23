// BlockExpression.cs in EternityChronicles/IronDragon
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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class BlockExpression : DragonExpression
    {
        internal BlockExpression(List<Expression> body, DragonScope scope)
        {
            body.RemoveAll(e => e == null);

            Body = body;
            SetScope(scope);
        }

        internal BlockExpression(List<Expression> body)
        {
            Body = body;
        }

        public List<Expression> Body { get; internal set; }

        public override Type Type => Body.Last().Type;

        public override void SetChildrenScopes(DragonScope scope)
        {
            foreach (var expr in Body) expr.SetScope(scope);
        }

        public override Expression Reduce()
        {
            return Block(Body);
        }

        public override string ToString()
        {
            var str = new StringBuilder("{ ");
            foreach (var e in Body)
            {
                str.Append(e);
                str.Append("; ");
            }

            str.Append("}");
            return str.ToString();
        }
    }
}