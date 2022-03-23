// DragonGlobalScope.cs in EternityChronicles/IronDragon
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

namespace IronDragon.Runtime
{
    public sealed class DragonGlobalScope : DragonScope
    {
        public override DragonScope ParentScope => null;

        protected override bool CheckAliases(string name)
        {
            return Aliases.ContainsKey(name);
        }

        protected override bool CheckConstant(string name)
        {
            return Constants.Contains(name);
        }

        protected override DragonScope GetAlias(string name)
        {
            return CheckAliases(name) ? this : null;
        }

        protected override dynamic Resolve(string name)
        {
            if (CheckAliases(name)) name = Aliases[name];
            return Variables.ContainsKey(name) ? Variables[name] : null;
        }

        protected override dynamic Resolve(Symbol sym)
        {
            return SymVars.ContainsKey(sym) ? SymVars[sym] : Resolve(sym.Name);
        }
    }
}