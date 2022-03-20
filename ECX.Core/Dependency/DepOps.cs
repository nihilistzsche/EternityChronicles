// DepOps.cs
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
    ///     Represents the dependency operators.
    /// </summary>
    /// <remarks>None.</remarks>
    public enum DepOps
    {
        /// <summary>
        ///     And operator.  Dependency is fulfilled if all given dependencies are fulfilled, otherwise will be unresolved if any
        ///     given dependency is unresolved.
        /// </summary>
        /// <example>
        ///     <code>(&amp;&amp; (## module1) (## module2))</code>
        /// </example>
        And,

        /// <summary>
        ///     Or operator.  Dependency is fulfilled if at least one given dependency is fulfilled, will be unresolved if all
        ///     given dependencies are unresolved.
        /// </summary>
        /// <example>
        ///     <code>(|| (## module1) (## module2))</code>
        /// </example>
        Or,

        /// <summary>
        ///     Exclusive or operator.  Dependency is fulfilled if any given dependency is fulfilled, with the exceptions it will
        ///     be unresolved if all dependencies are fulfilled or all dependencies are unresolved.
        /// </summary>
        /// <example>
        ///     <code>(^^ (## module1) (## module2))</code>
        /// </example>
        Xor,

        /// <summary>
        ///     The optional operator.  Will cause the given dependencies to always be fulfilled as they can be considered
        ///     optional.
        /// </summary>
        /// <example>
        ///     <code>(?? (## module1))</code>
        /// </example>
        Opt,

        /// <summary>
        ///     Equal operator.  Checks versions and is fulfilled only if the found assembly matches the given version.
        /// </summary>
        /// <example>
        ///     <code>(== module1 1.0)</code>
        /// </example>
        Equal,

        /// <summary>
        ///     The not equal operator.  Checks versions and is fulfilled only if the found assembly does not match the given
        ///     version.
        /// </summary>
        /// <example>
        ///     <code>(!= module1 1.0)</code>
        /// </example>
        NotEqual,

        /// <summary>
        ///     The less than equal operator.  Checks versions and is fulfilled only if the found assembly has a version equal to
        ///     or lower than the given version.
        /// </summary>
        /// <example>
        ///     <code>(&lt;= module1 1.0)</code>
        /// </example>
        LessThanEqual,

        /// <summary>
        ///     The less than operator.  Checks versions and is fulfilled only if the found assembly has a version lower than the
        ///     given version.
        /// </summary>
        /// <example>
        ///     <code>(&lt;&lt; module1 1.0)</code>
        /// </example>
        LessThan,

        /// <summary>
        ///     The greater than equal operator.  Checks versions and is fulfilled only if the found assembly has a version equal
        ///     to or greater than the given version.
        /// </summary>
        /// <example>
        ///     <code>(&gt;= module1 1.0)</code>
        /// </example>
        GreaterThanEqual,

        /// <summary>
        ///     The greater than operator.  Checks versions and is fulfilled only if the found assembly has a version greater than
        ///     the given version.
        /// </summary>
        /// <example>
        ///     <code>(&gt;&gt; module1 1.0)</code>
        /// </example>
        GreaterThan,

        /// <summary>
        ///     The loaded operator.  Is fulfilled if the given module can be found and loaded, otherwise fails.
        /// </summary>
        Loaded,

        /// <summary>
        ///     The not loaded operator.  Is fulfilled if the given module is either a) not loaded, or b) can be unloaded,
        ///     otherwise is unresolved.
        /// </summary>
        NotLoaded,

        /// <summary>
        ///     The null operator.  Serves only to initialize dependency nodes with no operator and to represent the empty
        ///     operator.
        /// </summary>
        Null
    }
}