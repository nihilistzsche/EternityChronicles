﻿// ClassOpenExpression.cs in EternityChronicles/IronDragon
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