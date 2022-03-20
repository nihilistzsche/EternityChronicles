// DragonInstance.cs
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

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public partial class DragonInstance
    {
        public DragonInstance(DragonClass @class)
        {
            _class = @class;
            SingletonMethods = new Dictionary<string, DragonFunction>();
            UndefinedMethods = new List<string>();
            RemovedMethods = new List<string>();
            InstanceVariables = new DragonScope();
        }

        public override string ToString()
        {
            return
                $"[DragonInstance: SingletonMethods={SingletonMethods}, InstanceVariables={InstanceVariables}, UndefinedMethods={UndefinedMethods}, RemovedMethods={RemovedMethods}, Class={Class}, Scope={Scope}]";
        }

        #region Properties

        internal DragonClass _class;

        public Dictionary<string, DragonFunction> SingletonMethods { get; }

        public DragonScope InstanceVariables { get; }

        public List<string> UndefinedMethods { get; }

        public List<string> RemovedMethods { get; }

        public DragonClass Class => _class;

        internal object BackingObject { get; set; }

        #endregion
    }
}