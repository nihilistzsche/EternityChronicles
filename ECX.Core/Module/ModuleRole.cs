//
// ModuleRole.cs
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
using System.Reflection;

namespace ECX.Core.Module
{
    /// <summary>
    /// Handler used to register a new producer of the role.
    /// </summary>
    /// <remarks>See <see href="roles.html">Roles</see> for more information on roles.</remarks>
    public delegate void RoleRegisterHandler (Assembly asm, Type basetype);
	
	/// <summary>
	/// Handler used to unregister a producer of the role.
	/// </summary>
	/// <remarks>See <see href="roles.html">Roles</see> for more information on roles.</remarks>
	public delegate void RoleUnregisterHandler (Assembly asm);
	
	/// <summary>
	/// This represents a role that modules can fulfill.
	/// </summary>
	/// <remarks>See <see href="roles.html">Roles</see> for more information on roles.</remarks>
	public class ModuleRole {
		private Type _baseType;
		private string _roleName;
		private RoleRegisterHandler _regHandler;
		private RoleUnregisterHandler _unregHandler;
		
		/// <summary>
		/// Creates a new ModuleRole argument.
		/// </summary>
		/// <remarks>None.</remarks>
		/// <param name="name">The name of the role.</param>
		/// <param name="basetype">The basetype of the role.</param>
		/// <param name="regHandler">The registration handler for the role.</param>
		/// <param name="unregHandler">The unregistration handler for the role.</param>
		public ModuleRole (string name, Type basetype, RoleRegisterHandler regHandler, RoleUnregisterHandler unregHandler) {
			_baseType = basetype;
			_roleName = name;
			_regHandler = regHandler;
			_unregHandler = unregHandler;
		}
		
		/// <summary>
		/// Gets the base type of the role.
		/// </summary>
		/// <remarks>None.</remarks>
		public Type BaseType => _baseType;

		/// <summary>
		/// Gets the name of the role.
		/// </summary>
		/// <remarks>None.</remarks>
		public string RoleName => _roleName;

		/// <summary>
		/// Gets the registration handler for the role.
		/// </summary>
		/// <remarks>None.</remarks>
		public RoleRegisterHandler RegistrationHandler => _regHandler;

		/// <summary>
		/// Gets the registration handler for the role.
		/// </summary>
		/// <remarks>None.</remarks>
		public RoleUnregisterHandler UnregistrationHandler => _unregHandler;
	}
}
