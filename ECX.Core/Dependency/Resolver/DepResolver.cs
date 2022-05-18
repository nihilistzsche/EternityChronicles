// DepResolver.cs in EternityChronicles/ECX.Core
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
using System.IO;
using System.Text;
using ECX.Core.Loader;
using ECX.Core.Module;

namespace ECX.Core.Dependency.Resolver
{
    /// <summary>
    ///     The DepResolver class handles resolving
    ///     dependencies given a dependency tree, and loads needed
    ///     modules or throws exceptions.  For a description of
    ///     search paths see <see href="searching.html">this</see>.
    /// </summary>
    /// <remarks>None.</remarks>
    /// <preliminary />
    public class DepResolver
    {
        #region Constructor

        /// <summary>
        ///     Creates a new DepResolver class with the the given
        ///     <see cref="ModuleController" /> object and search path.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="controller">The <see cref="ModuleController" /> object used to load modules.</param>
        /// <param name="search_path">The initial search path.</param>
        public DepResolver(ModuleController controller, List<string> searchPath)
        {
            Controller = controller;
            SearchPath = searchPath;
        }

        #endregion

        #region Members

        /// <summary>
        ///     A <see cref="ModuleController" /> object that is used for loading dependencies.
        /// </summary>
        /// <remarks>None.</remarks>
        protected ModuleController Controller;

        /// <summary>
        ///     A list of directories to search for modules.
        /// </summary>
        protected List<string> SearchPath;

        #endregion

        #region Internal Helper Functions

        /// <summary>
        ///     Searches for a given module along the search path.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_name">The name of the module to search for, without the .dll extension.</param>
        /// <returns>Returns a string giving the full path to the module or null if the module was not found.</returns>
        protected string SearchForModule(string name)
        {
            foreach (var s in SearchPath)
            {
                if (Directory.Exists(s))
                    foreach (var f in Directory.GetFiles(s, "*.dll"))
                    {
                        var g = f.Replace(s, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
                        if (g[..^4] == name) return s + Path.DirectorySeparatorChar + g;
                    }
            }

            return null;
        }

        /// <summary>
        ///     Recursively resolves dependencies given a dependency tree.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_node">The root node of the tree to resolve.</param>
        /// <param name="_parents">The parent modules to use when checking for circular dependencies.</param>
        /// <param name="_info">
        ///     The <see cref="ECX.Core.Module.ModuleInfo" /> object representing the module to resolve
        ///     dependencies for.
        /// </param>
        /// <param name="checking">
        ///     If set to true modules will not be loaded as we are only checking dependencies, otherwise they
        ///     will be loaded.
        /// </param>
        /// <param name="opt">Set to true if this pass is optional, otherwise set to false.</param>
        /// <exception cref="UnresolvedDependencyException">Thrown if the given dependency cannot be resolved and is not optional.</exception>
        /// <exception cref="CircularDependencyException">Thrown if two modules depend on each other.</exception>
        protected void OpResolve(DepNode node, List<string> parents, ModuleInfo info, bool checking, bool opt)
        {
            bool ret;
            var op = node.DepOp;

            // This is a large process.  I've chosen to use recursion here because it makes sense from a tree
            // point of view.  Our first step is to check our operator to see if its one of the "combination"
            // operators.  If it is, we simply recurse through its children and record their results in a
            // table (true if they suceeded, false if they failed).  Since we're not sure what kind of logic
            // we need (and, or, xor) we can't really make a decision at this point.  When we go through
            // the children, they simply go through this process to until we reach a bottom node, which would
            // be one of the "single" operators.  They are dealt with in the second part of this function.
            if (op is DepOps.And or DepOps.Opt or DepOps.Or or DepOps.Xor)
            {
                // This is our list of results for the children nodes.
                var results = new List<bool>();

                // This is a list of constraints for the children nodes so we can output sensible information.
                var c = new List<DepConstraint>();

                foreach (var child in node.Children) // Try to resolve the child node, if it suceeds, store
                    // true into the results table, if an exception is thrown
                    // store false into the results table.  Store the constraints
                    // into the constraints table regardless of result so the indexes
                    // will be identical.
                {
                    try
                    {
                        OpResolve(child, parents, info, checking, op == DepOps.Opt);
                        results.Add(true);
                        c.Add(child.Constraint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to resolve op with exception: {0}", e.Message);
                        results.Add(false);
                        c.Add(child.Constraint);
                    }
                }

                // This switch statement is where we go through the results table and apply
                // the operator logic to the table to figure out if the results sastify
                // the given operator.
                switch (op)
                {
                case DepOps.And:
                    var r = 0;

                    foreach (var result in results)
                    {
                        if (!result)
                            if (!opt)
                            {
                                if (c[r] != null)
                                    throw new UnresolvedDependencyException(
                                                                            string.Format(
                                                                             "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                             info.Name, c[r].Name, c[r].Version,
                                                                             OpToString(DepOps.And))
                                                                           );
                                throw new UnresolvedDependencyException(
                                                                        $"The following dependency for the module {info.Name} could not be resolved: ({OpToString(DepOps.And)} operator)"
                                                                       );
                            }

                        r++;
                    }

                    break;
                case DepOps.Opt: // This is optional so stuff is true regardless
                    break;
                case DepOps.Or:
                    ret = false;
                    var urexc = new List<string>();
                    r = 0;
                    foreach (var result in results)
                    {
                        if (result)
                        {
                            ret = true;
                            break;
                        }

                        if (c[r] != null)
                            urexc.Add($"{c[r].Name} ({c[r].Version})");
                        r++;
                    }

                    if (!ret)
                    {
                        var sb = new StringBuilder(
                                                    $"The following dependency for the module {info.Name} could not be resolved: (OR operator)\n"
                                                   );
                        urexc.ForEach(innerException => sb.Append($"\t{innerException}\n"));
                        if (!opt)
                            throw new UnresolvedDependencyException(sb.ToString());
                    }

                    break;

                case DepOps.Xor:
                    var xt = true;
                    var xf = true;
                    var xexc = new List<string>();

                    r = 0;
                    ret = true;

                    foreach (var result in results)
                    {
                        if (result)
                        {
                            xf = false;
                            if (c[r] != null)
                                xexc.Add($"{c[r].Name} ({c[r].Version}) (True)");
                        }

                        if (!result)
                        {
                            xt = false;
                            if (c[r] != null)
                                xexc.Add($"{c[r].Name} ({c[r].Version}) (False)");
                        }

                        r++;
                    }

                    // If one of these is still true, that means all of the other results are true or false.
                    if (xt || xf)
                        ret = false;

                    if (!ret)
                    {
                        var sb = new StringBuilder(
                                                    $"The following dependency for the module {info.Name} could not be resolved: (XOR operator)\n"
                                                   );
                        xexc.ForEach(innerException => sb.Append($"\t{innerException}\n"));
                        if (!opt) throw new UnresolvedDependencyException(sb.ToString());
                    }

                    break;
                }
            }
            else
            {
                var constraint = node.Constraint;

                foreach (var parent in parents)
                {
                    if (parent == constraint.Name)
                        throw new CircularDependencyException(
                                                              $"{parent} and {info.Name} have a circular dependency on each other");
                }

                // single operators
                if (SearchForModule(constraint.Name) == null)
                    ret = false;

                ModuleInfo ninfo = null;

                var loader = new ModuleLoader(SearchPath, Controller);

                try
                {
                    if (!parents.Contains(info.Name))
                        parents.Add(info.Name);

                    loader.LoadModule(parents, constraint.Name, out ninfo, true, true);
                }
                catch (CircularDependencyException ce)
                {
                    throw ce;
                }
                catch (Exception)
                {
                    if (!opt)
                        throw new UnresolvedDependencyException(
                                                                string.Format(
                                                                              "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                              info.Name, constraint.Name,
                                                                              constraint.Version, OpToString(op))
                                                               );
                }

                if (op is DepOps.Equal or DepOps.GreaterThan or DepOps.GreaterThanEqual or DepOps.LessThan or DepOps.LessThanEqual or DepOps.NotEqual)
                {
                    if (!IsEmptyVersion(constraint.Version))
                        if (!VersionMatch(ninfo.Version, constraint.Version, op))
                            if (!opt)
                                throw new UnresolvedDependencyException(
                                                                        string.Format(
                                                                         "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                         info.Name, constraint.Name,
                                                                         constraint.Version, OpToString(op))
                                                                       );

                    if (!checking)
                    {
                        Controller.LoadModule(constraint.Name);

                        parents.Sort();

                        var seen = new List<string>
                                    {
                                        info.Name,
                                        constraint.Name
                                    };
                        foreach (var p in parents)
                        {
                            if (!seen.Contains(p))
                            {
                                Controller.IncRef(constraint.Name);
                                seen.Add(p);
                            }
                        }
                    }
                }

                if (op == DepOps.Loaded)
                {
                    if (Controller.Loader.SearchForModule(constraint.Name) == null)
                        if (!opt)
                            throw new UnresolvedDependencyException(
                                                                    string.Format(
                                                                     "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                     info.Name, constraint.Name,
                                                                     constraint.Version, OpToString(op))
                                                                   );

                    if (!checking)
                    {
                        Controller.LoadModule(constraint.Name);

                        parents.Sort();

                        var seen = new List<string>
                                    {
                                        info.Name,
                                        constraint.Name
                                    };
                        foreach (var p in parents)
                        {
                            if (!seen.Contains(p))
                            {
                                Controller.IncRef(constraint.Name);
                                seen.Add(p);
                            }
                        }
                    }
                }

                if (op == DepOps.NotLoaded)
                    if (Controller.IsLoaded(constraint.Name))
                    {
                        if (Controller.RefCount(constraint.Name) > 1)
                            if (!opt)
                                throw new UnresolvedDependencyException(
                                                                        string.Format(
                                                                         "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                         info.Name, constraint.Name,
                                                                         constraint.Version, OpToString(op))
                                                                       );
                        if (!checking) Controller.UnloadModule(constraint.Name);
                    }
            }
        }

        /// <summary>
        ///     Converts a given <see cref="DepOps" /> object into a string.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_op">The operator to convert to a string.</param>
        /// <returns>A string value that matches the operator in the <see href="depstring.html">dependency operator table</see>.</returns>
        protected static string OpToString(DepOps op)
        {
            return op switch
                   {
                       DepOps.And => "&&",
                       DepOps.Equal => "==",
                       DepOps.GreaterThan => ">>",
                       DepOps.GreaterThanEqual => ">=",
                       DepOps.LessThan => "<<",
                       DepOps.LessThanEqual => "<=",
                       DepOps.Loaded => "##",
                       DepOps.NotLoaded => "!#",
                       DepOps.NotEqual => "!=",
                       DepOps.Opt => "??",
                       DepOps.Or => "||",
                       DepOps.Xor => "^^",
                       _ => ""
                   };
        }

        /// <summary>
        ///     Determines if a given version is empty or not.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_ver">A <see cref="DepVersion" /> object to check.</param>
        /// <returns>Returns true if the version given is empty (ie -1:-1:-1:-1) or false otherwise.</returns>
        protected bool IsEmptyVersion(DepVersion version)
        {
            if (version == null)
                return true;

            return version.Major == -1 && version.Minor == -1 && version.Build == -1 && version.Revision == -1;
        }

        /// <summary>
        ///     Checks if the version given matches the constraint
        ///     based on the given operator.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_ver">The version to compare to the constraint.</param>
        /// <param name="_dver">The constraint version.</param>
        /// <param name="_op">The operator to use to determine if the match is sucessful.</param>
        /// <returns>Returns true is the version matches the constraint version with the given operator, otherwise returns false.</returns>
        protected bool VersionMatch(DepVersion moduleVersion, DepVersion dependencyVersion, DepOps op)
        {
            Version mver, drver;

            if (dependencyVersion.Major == -1)
                return true;

            mver = new Version(dependencyVersion.Major != -1 ? moduleVersion.Major : 0,
                                dependencyVersion.Minor != -1 ? moduleVersion.Minor : 0,
                                dependencyVersion.Build != -1 ? moduleVersion.Build : 0,
                                dependencyVersion.Revision != -1 ? moduleVersion.Revision : 0);

            drver = new Version(dependencyVersion.Major != -1 ? dependencyVersion.Major : 0,
                                 dependencyVersion.Minor != -1 ? dependencyVersion.Minor : 0,
                                 dependencyVersion.Build != -1 ? dependencyVersion.Build : 0,
                                 dependencyVersion.Revision != -1 ? dependencyVersion.Revision : 0);

            var vres = mver.CompareTo(drver);

            return op switch
                   {
                       DepOps.Equal => vres == 0,
                       DepOps.GreaterThan => vres > 0,
                       DepOps.GreaterThanEqual => vres >= 0,
                       DepOps.LessThan => vres < 0,
                       DepOps.LessThanEqual => vres <= 0,
                       DepOps.NotEqual => vres != 0,
                       DepOps.Loaded => true,
                       _ => false
                   };
        }

        /// <summary>
        ///     Helper function that is the actual resolving function.
        ///     See <see cref="ResolveCheck" /> and <see cref="Resolve" /> for
        ///     the API to call this method.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_parents">The list of parent modules to use for checking for circular dependencies.</param>
        /// <param name="_info">The <see cref="ModuleInfo" /> object representing the current module.</param>
        /// <param name="checking">A bool to determine whether or not we are just checking, or actually loading.</param>
        protected void InternalResolve(List<string> parents, ModuleInfo info, bool checking)
        {
            if (info.Dependencies == null)
                return;

            OpResolve(info.Dependencies, parents ?? new List<string>(), info, checking, false);
        }

        #endregion

        #region Resolvers

        /// <summary>
        ///     Performs a resolution check without actually loading anything.
        ///     This is useful to determine if the dependencies can be resolved
        ///     before actually loading anything.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_parents">The list of parent modules to use for checking for circular dependencies.</param>
        /// <param name="_info">A <see cref="ModuleInfo" /> object representing the current module.</param>
        public void ResolveCheck(List<string> parents, ModuleInfo info)
        {
            InternalResolve(parents, info, true);
        }

        /// <summary>
        ///     Performs a resolution check and loads the modules needed
        ///     to sastify the dependencies.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_parents">The list of parent modules to use for checking for circular dependencies.</param>
        /// <param name="_info">A <see cref="ModuleInfo" /> object representing the current module.</param>
        public void Resolve(List<string> parents, ModuleInfo info)
        {
            InternalResolve(parents, info, false);
        }

        #endregion
    }
}