// IScopeExpression.cs in EternityChronicles/IronDragon
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
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public interface IScopeExpression
    {
        DragonScope Scope { get; }

        void SetScope(DragonScope scope);
    }

    public static class ScopeExpressionExtension
    {
        public static void SetScope(this Expression expr, DragonScope scope)
        {
            expr.SetParentScopeIfBlock(scope);
        }

        private static void SetParentScope(this Expression expr, DragonScope scope)
        {
            if (!(expr is BlockExpression block)) return;
            if (block.Scope == null) block.Scope = new DragonScope();
            block.Scope.ParentScope = scope;
        }

        private static void SetParentScopeIfBlock(this Expression expr, DragonScope scope)
        {
            if (expr is BlockExpression)
                expr.SetParentScope(scope);
            else if (expr is IScopeExpression) (expr as IScopeExpression).SetScope(scope);
        }
    }
}