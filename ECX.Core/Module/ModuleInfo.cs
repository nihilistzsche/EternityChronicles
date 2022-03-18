//
// ModuleInfo.cs
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

using System.Linq;
using System.Reflection;
using Antlr.Runtime;
using ECX.Core.Dependency;
using ECX.Core.Dependency.Parser;

namespace ECX.Core.Module
{
    /// <summary>
    ///     Represents information about a module.
    /// </summary>
    /// <remarks>None.</remarks>
    public record ModuleInfo
    {
        /// <summary>
        ///     Creates a new ModuleInfo belonging to the given assembly.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_asm">The owning assembly.</param>
        public ModuleInfo(Assembly assembly)
        {
            Name = assembly.GetName().Name;
            Version = DepVersion.VersionParse(assembly.GetName().Version.ToString());

            var _depAttr = assembly.GetCustomAttributes(typeof(ModuleDependencyAttribute), false);

            if (_depAttr.Any())
            {
                var _lexer =
                    new ECXLexer(new ANTLRStringStream(((ModuleDependencyAttribute)_depAttr.First()).DepString));
                var _parser = new ECXParser(new CommonTokenStream(_lexer));
                Dependencies = _parser.expr();
            }
            else
            {
                Dependencies = null;
            }

            var _roleAttr = assembly.GetCustomAttributes(typeof(ModuleRoleAttribute), false);

            if (_roleAttr.Any())
                Roles = ((ModuleRoleAttribute)_roleAttr.First()).Roles;
            else
                Roles = null;

            Owner = assembly;
        }

        /// <summary>
        ///     Gets the modules name.
        /// </summary>
        /// <remarks>None.</remarks>
        public string Name { get; }

        /// <summary>
        ///     Gets the modules version.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepVersion Version { get; }

        /// <summary>
        ///     Gets the modules dependency tree.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepNode Dependencies { get; }

        /// <summary>
        ///     Gets the modules roles.
        /// </summary>
        /// <remarks>See <see href="roles.html">Roles</see> for more information on module roles.</remarks>
        public string Roles { get; }

        /// <summary>
        ///     Gets the owning assembly.
        /// </summary>
        /// <remarks>None.</remarks>
        public Assembly Owner { get; }
    }
}