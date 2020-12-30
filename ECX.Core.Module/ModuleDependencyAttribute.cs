//
// ModuleDependencyAttribute.cs
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

 
namespace ECX.Core {
	using System;

	/// <summary>
	/// Holds a string representation of a modules dependency's.
	/// </summary>
	/// <remarks>
	/// This attribute is only valid on assembly targets.
	/// See <see href="depstring.html" /> for information on the format of
	/// dependency strings and a description of the dependency operators.
	/// </remarks>
	/// <preliminary />
	[AttributeUsage(AttributeTargets.Assembly)]
	public class ModuleDependencyAttribute : Attribute {
		/// <summary>
		/// Creates a new ModuleDependencyAttribute object using the given dep string.
		/// </summary>
		/// <remarks>None.</remarks>
		/// <param name="dep_string">A string representing the module's dependencies.</param>
		public ModuleDependencyAttribute (string depString) {
			DepString = depString;
		}
		
		/// <summary>
		/// Returns the dependency string of a ModuleDependencyAttribute object.
		/// </summary>
		/// <remarks>None.</remarks>
		public string DepString { get; protected set; }
	}
}
