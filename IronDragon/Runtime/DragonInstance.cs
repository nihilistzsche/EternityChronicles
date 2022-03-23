// DragonInstance.cs in EternityChronicles/IronDragon
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