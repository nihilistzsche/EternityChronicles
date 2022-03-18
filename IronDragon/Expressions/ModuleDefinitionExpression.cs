using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    using CS = CompilerServices;

    public class ModuleDefinitionExpression : DragonExpression
    {
        internal ModuleDefinitionExpression(string name, List<Expression> contents)
        {
            Name = name;
            Contents = contents;
        }

        public string Name { get; }

        public List<Expression> Contents { get; }

        public override Type Type => typeof(DragonModule);

        public override string ToString()
        {
            return "";
        }

        public override Expression Reduce()
        {
            return Operation.DefineModule(typeof(DragonModule), Constant(Name), Constant(Contents), Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Contents.ForEach(content => content.SetScope(scope));
        }
    }
}