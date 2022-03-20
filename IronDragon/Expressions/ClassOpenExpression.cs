// ClassOpenExpression.cs
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronDragon.Runtime;

namespace IronDragon.Expressions
{
    using CS = CompilerServices;

    public class ClassOpenExpression : DragonExpression
    {
        internal ClassOpenExpression(Expression name,
                                     List<Expression> contents)
        {
            Name = name;
            Contents = contents;
        }

        public Expression Name { get; }

        public List<Expression> Contents { get; }

        public override Type Type => typeof(DragonClass);

        public override string ToString()
        {
            return "";
        }

        public override Expression Reduce()
        {
            return Operation.DefineClassOpen(typeof(DragonClass), Constant(Name), Constant(Contents),
                                             Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            Name.SetScope(scope);
            Contents.ForEach(content => content.SetScope(scope));
        }
    }
}