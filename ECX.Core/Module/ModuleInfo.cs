// ModuleInfo.cs in EternityChronicles/ECX.Core
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