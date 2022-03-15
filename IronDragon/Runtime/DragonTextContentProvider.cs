// -----------------------------------------------------------------------
// <copyright file="DragonTextContentProvider.cs" company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
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
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.Scripting;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class DragonTextContentProvider : TextContentProvider
    {
        public DragonTextContentProvider(string source)
        {
            Source = source;
        }

        public string Source { get; }

        public override SourceCodeReader GetReader()
        {
            return new SourceCodeReader(new StringReader(Source), Encoding.UTF8);
        }
    }
}