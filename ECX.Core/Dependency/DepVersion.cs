// DepVersion.cs in EternityChronicles/ECX.Core
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
    ///     Represents a dependency's version in an opaque structure.
    /// </summary>
    /// <remarks>None.</remarks>
    public class DepVersion
    {
        /// <summary>
        ///     Creates a new version with the given information.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="build">The build version number.</param>
        /// <param name="revision">The revision version number.</param>
        public DepVersion(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        /// <summary>
        ///     Creates a new version with the given information.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="build">The build version number.</param>
        public DepVersion(int major, int minor, int build)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = -1;
        }

        /// <summary>
        ///     Creates a new version with the given information.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public DepVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
            Build = -1;
            Revision = -1;
        }

        /// <summary>
        ///     Creates a new empty version.
        /// </summary>
        /// <remarks>None.</remarks>
        public DepVersion()
        {
            Major = -1;
            Minor = -1;
            Build = -1;
            Revision = -1;
        }

        /// <summary>
        ///     Gets or sets the major version number.
        /// </summary>
        /// <remarks>None.</remarks>
        public int Major { get; set; }

        /// <summary>
        ///     Gets or sets the minor version number.
        /// </summary>
        /// <remarks>None.</remarks>
        public int Minor { get; set; }

        /// <summary>
        ///     Gets or sets the build version number.
        /// </summary>
        /// <remarks>None.</remarks>
        public int Build { get; set; }

        /// <summary>
        ///     Gets or sets the revision version number.
        /// </summary>
        /// <remarks>None.</remarks>
        public int Revision { get; set; }

        /// <summary>
        ///     Parses a string to generate a new DepVersion object.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="v">The string representation of the version.</param>
        public static DepVersion VersionParse(string v)
        {
            // Here we go :)
            var ver = new DepVersion();
            var vparts = v.Split('.');
            ver.Major = int.Parse(vparts[0]);
            ver.Minor = int.Parse(vparts[1]);
            if (vparts.Length > 2)
                ver.Build = int.Parse(vparts[2]);
            if (vparts.Length > 3)
                ver.Revision = int.Parse(vparts[3]);
            return ver;
        }

        /// <summary>
        ///     Converts the version into a string.
        /// </summary>
        /// <remarks>The output is the same format as CIL version strings, i.e. 1:0:0:0.</remarks>
        /// <returns>Returns a string representation of the version.</returns>
        public override string ToString()
        {
            return $"{Major}:{Minor}:{Build}:{Revision}";
        }
    }
}