// DragonFunction.cs in EternityChronicles/IronDragon
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

using System.Collections.Generic;
using IronDragon.Expressions;

// <copyright file="DragonFunction.cs" Company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public partial class DragonFunction
    {
        public DragonFunction(string name, List<FunctionArgument> arguments, BlockExpression body, DragonScope context)
        {
            Name = name;
            Arguments = arguments;
            Body = body;
            Context = context;
        }

        public override string ToString()
        {
            return string.Format("[DragonFunction: Arguments={0}, Name={1}, Body={2}, Context={3}, Scope={4}]",
                                 Arguments, Name, Body, Context, Scope);
        }

        #region Properties

        public List<FunctionArgument> Arguments { get; internal set; }

        public string Name { get; internal set; }

        public BlockExpression Body { get; internal set; }

        public DragonScope Context { get; internal set; }

        internal bool IsYieldBlock { get; set; }

        internal string SingletonName { get; set; }

        internal bool IsSingleton { get; set; }

        #endregion
    }
}