// DragonGlobalScope.cs in EternityChronicles/IronDragon
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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