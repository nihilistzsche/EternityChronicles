// IModule.cs in EternityChronicles/ECX.Core.Module
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

namespace ECX.Core
{
    /// <summary>
    ///     This represents a module entry point class if a plug-in needs one.
    /// </summary>
    /// <remarks>
    ///     A module does not need a type that implements this interface, it is
    ///     only used if the modules need to do some supplementary information on
    ///     the controller, such as registering a new role, or retrieving a type
    ///     from a module which it depends on.
    /// </remarks>
    /// <preliminary />
    public interface IModule
    {
        /// <summary>
        ///     The entry point of the module.
        /// </summary>
        /// <remarks>
        ///     The <paramref name="controller" /> will always be a ModuleController object.
        ///     This method is called when a module is loaded.
        /// </remarks>
        /// <param name="controller">The module controller to be used for auxillary operations.</param>
        void ModuleEntry(object controller);

        /// <summary>
        ///     The exit point of the module.
        /// </summary>
        /// <remarks>
        ///     The <paramref name="controller" /> will always be a ModuleController object.
        ///     This method is called when a module is unloaded.
        /// </remarks>
        /// <param name="controller">The module controller to be used for auxillary operations.</param>
        void ModuleExit(object controller);
    }
}