// ModuleDependencyAttribute.cs in EternityChronicles/ECX.Core.Module
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
    ///     Holds a string representation of a modules dependency's.
    /// </summary>
    /// <remarks>
    ///     This attribute is only valid on assembly targets.
    ///     See <see href="depstring.html" /> for information on the format of
    ///     dependency strings and a description of the dependency operators.
    /// </remarks>
    /// <preliminary />
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleDependencyAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new ModuleDependencyAttribute object using the given dep string.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="dep_string">A string representing the module's dependencies.</param>
        public ModuleDependencyAttribute(string depString)
        {
            DepString = depString;
        }

        /// <summary>
        ///     Returns the dependency string of a ModuleDependencyAttribute object.
        /// </summary>
        /// <remarks>None.</remarks>
        public string DepString { get; protected set; }
    }
}