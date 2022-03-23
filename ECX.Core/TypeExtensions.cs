// TypeExtensions.cs in EternityChronicles/ECX.Core
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
using System.Collections.Generic;
using System.Linq;

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_Elsewhere

namespace ECX.Core
{
    /// <summary>
    ///     Provides extensions to <see cref="System.Type" />.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Creates a generic instance of a given type.
        /// </summary>
        /// <returns>The generic instance.</returns>
        /// <param name="type">The base type.</param>
        /// <param name="typeArgs">Generic type arguments.</param>
        /// <param name="args">Arguments for the constructor.</param>
        public static object CreateGenericInstance(this Type type, Type[] typeArgs, object[] args)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));
            var constructorTypes = new List<Type>();
            args.ToList().ForEach(arg => constructorTypes.Add(arg.GetType()));
            return genericTypeDefinition.MakeGenericType(typeArgs).GetConstructor(constructorTypes.ToArray())
                                        ?.Invoke(args);
        }
    }
}