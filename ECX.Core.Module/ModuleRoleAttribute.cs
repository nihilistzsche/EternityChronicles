// ModuleRoleAttribute.cs in EternityChronicles/ECX.Core.Module
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

namespace ECX.Core
{
    /// <summary>
    ///     Holds a string representation of a modules roles.
    /// </summary>
    /// <remarks>
    ///     This attribute is only valid on assembly targets.
    ///     The roles should be a comma-seperated list of roles that
    ///     the module provides facilities for.
    /// </remarks>
    /// <preliminary />
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleRoleAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new ModuleRoleAttribute with the given roles.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="role">Comma-seperate list of roles the module provides facilities for.</param>
        public ModuleRoleAttribute(string role)
        {
            Roles = role;
        }

        /// <summary>
        ///     Retrieves the list of roles from a ModuleRoleAttribute object.
        /// </summary>
        /// <remarks>None.</remarks>
        public string Roles { get; protected set; }
    }
}