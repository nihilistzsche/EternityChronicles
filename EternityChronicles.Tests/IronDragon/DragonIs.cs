// DragonIs.cs in EternityChronicles/EternityChronicles.Tests
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

// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using IronDragon.Runtime;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EternityChronicles.Tests.IronDragon
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class DragonIs : Is
    {
        public static FunctionConstraint Function(DragonFunction expected)
        {
            return new FunctionConstraint(expected);
        }
    }

    public static class ConstraintExpressionExtensions
    {
        public static FunctionConstraint Function(this ConstraintExpression expr, DragonFunction expected)
        {
            return DragonIs.Function(expected);
        }
    }
}