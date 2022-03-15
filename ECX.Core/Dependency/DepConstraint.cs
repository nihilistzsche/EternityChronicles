//
// DepConstraint.cs
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
            Name    = "";
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