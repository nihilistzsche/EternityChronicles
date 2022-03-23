// YieldCheckVisitor.cs in EternityChronicles/IronDragon
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

using System.Linq.Expressions;
using IronDragon.Expressions;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class YieldCheckVisitor : DragonExpressionVisitor
    {
        /// <summary>
        ///     Initializes a new instance of the {YieldCheckVisitor} class.
        /// </summary>
        public YieldCheckVisitor()
        {
            HasYield = false;
        }

        /// <summary>
        ///     Gets the paramter variables this visitor collects from the tree.
        /// </summary>
        public bool HasYield { get; private set; }

        protected override Expression VisitYield(YieldExpression node)
        {
            HasYield = true;
            return node;
        }
    }
}