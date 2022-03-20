// BeginExpression.cs in EternityChronicles/IronDragon
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
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class BeginExpression : DragonExpression
    {
        internal BeginExpression(Expression tryBlock, List<Expression> rescueBlocks, Expression ensureBlock,
                                 Expression elseBlock)
        {
            TryBlock = tryBlock;
            RescueBlocks = rescueBlocks;
            EnsureBlock = ensureBlock;
            ElseBlock = elseBlock;
        }

        public Expression TryBlock { get; }

        public List<Expression> RescueBlocks { get; }

        public Expression EnsureBlock { get; }

        public Expression ElseBlock { get; }

        public override Type Type => TryBlock.Type;

        // Should not actually reduce, used by the runtime directly
        public override Expression Reduce()
        {
            return Operation.Begin(Type, Constant(TryBlock), Constant(RescueBlocks), Constant(EnsureBlock),
                                   Constant(ElseBlock), Constant(Scope));
        }

        public override void SetChildrenScopes(DragonScope scope)
        {
            TryBlock.SetScope(scope);
            RescueBlocks.ForEach(block => block.SetScope(scope));
            EnsureBlock.SetScope(scope);
            ElseBlock.SetScope(scope);
        }

        public override string ToString()
        {
            return "";
        }
    }
}