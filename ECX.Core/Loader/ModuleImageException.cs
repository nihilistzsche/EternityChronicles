// ModuleImageException.cs
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

using System;

namespace ECX.Core.Loader
{
    /// <summary>
    ///     Thrown if a loaded module is a bad image.
    /// </summary>
    /// <remarks>None.</remarks>
    /// <preliminary />
    public class ModuleImageException : Exception
    {
        /// <summary>
        ///     Creates a new ModuleImageException object.
        /// </summary>
        /// <remarks>None.</remarks>
        public ModuleImageException()
        {
        }

        /// <summary>
        ///     Creates a new ModuleImageException object with the given message.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="message">The message to be given when the execption is thrown.</param>
        public ModuleImageException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Creates a new ModuleImageException object with the given message and inner exception.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="message">The message to be given when the exception is thrown.</param>
        /// <param name="innerException">The inner exception of this exception.</param>
        public ModuleImageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}