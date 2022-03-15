//
// ModuleRoleAttribute.cs
//
// Author:
//     Michael Tindal <mj.tindal@icloud.com>
//
// Copyright (C) 2005-2013 Michael Tindal and the individuals listed on
// the ChangeLog entries.
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
//


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