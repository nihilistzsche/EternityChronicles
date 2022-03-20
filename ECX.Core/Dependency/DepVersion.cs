// DepVersion.cs
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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