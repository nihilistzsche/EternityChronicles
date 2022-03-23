// CircularDependencyException.cs in EternityChronicles/ECX.Core
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

using System;

namespace ECX.Core.Dependency.Resolver
{
    /// <summary>
    ///     Exception thrown when a circular dependency is detected.
    /// </summary>
    /// <remarks>
    ///     A circular dependency is defined as any two modules depending on
    ///     each other.  ECX does not support circular dependencies.
    ///     Examples of these include:
    ///     <pre> Module A depends on Module B, Module B depends on Module A </pre>
    ///     <pre> Module A depends on Module B, Module B depends on Module C, Module C depends on Module A </pre>
    ///     <pre> Module A depends on Module A </pre>
    ///     I don't know why someone would have a module depend on itself, but the resolver
    ///     does detect the situation.
    /// </remarks>
    /// <preliminary />
    public class CircularDependencyException : Exception
    {
        /// <summary>
        ///     Creates a new CircularDependencyException object.
        /// </summary>
        /// <remarks>None.</remarks>
        public CircularDependencyException()
        {
        }

        /// <summary>
        ///     Creates a new CircularDependencyException object with the given message.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="message">The message to be given when the execption is thrown.</param>
        public CircularDependencyException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Creates a new CircularDependencyException object with the given message and inner exception.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="message">The message to be given when the exception is thrown.</param>
        /// <param name="innerException">The inner exception of this exception.</param>
        public CircularDependencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}