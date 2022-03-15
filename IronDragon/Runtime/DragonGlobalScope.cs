using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using IronDragon.Builtins;
using IronDragon.Expressions;
using BlockExpression = IronDragon.Expressions.BlockExpression;

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

        protected override dynamic Resolve(string name) {
            if (CheckAliases(name)) {
                name = Aliases[name];
            }
            return Variables.ContainsKey(name) ? Variables[name] : null;
        }

        protected override dynamic Resolve(Symbol sym)
        {
            return SymVars.ContainsKey(sym) ? SymVars[sym] : Resolve(sym.Name);
        }

        public dynamic GetVariable(string varName)
        { 
            return DragonScriptCode.Convert(Resolve(varName), this);
        }
    }
}