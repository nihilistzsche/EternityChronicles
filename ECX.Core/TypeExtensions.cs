// -----------------------------------------------------------------------
// <copyright file="AssemblyExtensions.cs" Company="${Company}">
// Copyright 2013 Michael Tindal
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