// ModuleRole.cs in EternityChronicles/ECX.Core
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
using System.Reflection;

namespace ECX.Core.Module
{
    /// <summary>
    ///     Handler used to register a new producer of the role.
    /// </summary>
    /// <remarks>See <see href="roles.html">Roles</see> for more information on roles.</remarks>
    public delegate void RoleRegisterHandler(Assembly asm, Type basetype);

    /// <summary>
    ///     Handler used to unregister a producer of the role.
    /// </summary>
    /// <remarks>See <see href="roles.html">Roles</see> for more information on roles.</remarks>
    public delegate void RoleUnregisterHandler(Assembly asm);

    /// <summary>
    ///     This represents a role that modules can fulfill.
    /// </summary>
    /// <remarks>See <see href="roles.html">Roles</see> for more information on roles.</remarks>
    public class ModuleRole
    {
        /// <summary>
        ///     Creates a new ModuleRole argument.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="name">The name of the role.</param>
        /// <param name="basetype">The basetype of the role.</param>
        /// <param name="regHandler">The registration handler for the role.</param>
        /// <param name="unregHandler">The unregistration handler for the role.</param>
        public ModuleRole(string name, Type basetype, RoleRegisterHandler regHandler,
                          RoleUnregisterHandler unregHandler)
        {
            BaseType = basetype;
            RoleName = name;
            RegistrationHandler = regHandler;
            UnregistrationHandler = unregHandler;
        }

        /// <summary>
        ///     Gets the base type of the role.
        /// </summary>
        /// <remarks>None.</remarks>
        public Type BaseType { get; }

        /// <summary>
        ///     Gets the name of the role.
        /// </summary>
        /// <remarks>None.</remarks>
        public string RoleName { get; }

        /// <summary>
        ///     Gets the registration handler for the role.
        /// </summary>
        /// <remarks>None.</remarks>
        public RoleRegisterHandler RegistrationHandler { get; }

        /// <summary>
        ///     Gets the registration handler for the role.
        /// </summary>
        /// <remarks>None.</remarks>
        public RoleUnregisterHandler UnregistrationHandler { get; }
    }
}