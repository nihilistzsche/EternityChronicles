// IModule.cs in EternityChronicles/ECX.Core.Module
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