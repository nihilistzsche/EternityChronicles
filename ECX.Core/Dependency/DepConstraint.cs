// DepConstraint.cs in EternityChronicles/ECX.Core
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

namespace ECX.Core.Dependency
{
    /// <summary>
    ///     Represents a dependency constraint.
    /// </summary>
    /// <remarks>None.</remarks>
    public class DepConstraint
    {
        /// <summary>
        ///     Creates a new DepConstraint object.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepConstraint()
        {
            Name = "";
            Version = new DepVersion(-1, -1, -1, -1);
        }

        /// <summary>
        ///     Gets or sets the needed version.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepVersion Version { get; set; }

        /// <summary>
        ///     Gets or sets the needed module name.
        /// </summary>
        /// <remarks>None.</remarks>
        public string Name { get; set; }

        /// <summary>
        ///     Sets the version based on a string representation of it.
        /// </summary>
        /// <param name="version">The version string</param>
        /// <remarks>None.</remarks>
        public void SetVersion(string version)
        {
            Version = DepVersion.VersionParse(version);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current <see cref="DepConstraint" />.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents the current <see cref="DepConstraint" />.</returns>
        public override string ToString()
        {
            return $"{Name}[{Version}]";
        }
    }
}