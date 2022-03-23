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
                        var _f = f.Replace(s, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
                        if (_f.Substring(0, _f.Length - 4) == name) return s + Path.DirectorySeparatorChar + _f;
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
            bool _ret;
            var _op = node.DepOp;

            // This is a large process.  I've chosen to use recursion here because it makes sense from a tree
            // point of view.  Our first step is to check our operator to see if its one of the "combination"
            // operators.  If it is, we simply recurse through its children and record their results in a
            // table (true if they suceeded, false if they failed).  Since we're not sure what kind of logic
            // we need (and, or, xor) we can't really make a decision at this point.  When we go through
            // the children, they simply go through this process to until we reach a bottom node, which would
            // be one of the "single" operators.  They are dealt with in the second part of this function.
            if (_op == DepOps.And || _op == DepOps.Opt || _op == DepOps.Or || _op == DepOps.Xor)
            {
                // This is our list of results for the children nodes.
                var _results = new List<bool>();

                // This is a list of constraints for the children nodes so we can output sensible information.
                var _c = new List<DepConstraint>();

                foreach (var _child in node.Children) // Try to resolve the child node, if it suceeds, store
                    // true into the results table, if an exception is thrown
                    // store false into the results table.  Store the constraints
                    // into the constraints table regardless of result so the indexes
                    // will be identical.
                {
                    try
                    {
                        OpResolve(_child, parents, info, checking, _op == DepOps.Opt);
                        _results.Add(true);
                        _c.Add(_child.Constraint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to resolve op with exception: {0}", e.Message);
                        _results.Add(false);
                        _c.Add(_child.Constraint);
                    }
                }

                // This switch statement is where we go through the results table and apply
                // the operator logic to the table to figure out if the results sastify
                // the given operator.
                switch (_op)
                {
                case DepOps.And:
                    var r = 0;

                    foreach (var _result in _results)
                    {
                        if (!_result)
                            if (!opt)
                            {
                                if (_c[r] != null)
                                    throw new UnresolvedDependencyException(
                                                                            string.Format(
                                                                             "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                             info.Name, _c[r].Name, _c[r].Version,
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
                    _ret = false;
                    var _urexc = new List<string>();
                    r = 0;
                    foreach (var _result in _results)
                    {
                        if (_result)
                        {
                            _ret = true;
                            break;
                        }

                        if (_c[r] != null)
                            _urexc.Add($"{_c[r].Name} ({_c[r].Version})");
                        r++;
                    }

                    if (!_ret)
                    {
                        var _sb = new StringBuilder(
                                                    $"The following dependency for the module {info.Name} could not be resolved: (OR operator)\n"
                                                   );
                        _urexc.ForEach(innerException => _sb.Append($"\t{innerException}\n"));
                        if (!opt)
                            throw new UnresolvedDependencyException(_sb.ToString());
                    }

                    break;

                case DepOps.Xor:
                    var _xt = true;
                    var _xf = true;
                    var _xexc = new List<string>();

                    r = 0;
                    _ret = true;

                    foreach (var _result in _results)
                    {
                        if (_result)
                        {
                            _xf = false;
                            if (_c[r] != null)
                                _xexc.Add($"{_c[r].Name} ({_c[r].Version}) (True)");
                        }

                        if (!_result)
                        {
                            _xt = false;
                            if (_c[r] != null)
                                _xexc.Add($"{_c[r].Name} ({_c[r].Version}) (False)");
                        }

                        r++;
                    }

                    // If one of these is still true, that means all of the other results are true or false.
                    if (_xt || _xf)
                        _ret = false;

                    if (!_ret)
                    {
                        var _sb = new StringBuilder(
                                                    $"The following dependency for the module {info.Name} could not be resolved: (XOR operator)\n"
                                                   );
                        _xexc.ForEach(innerException => _sb.Append($"\t{innerException}\n"));
                        if (!opt) throw new UnresolvedDependencyException(_sb.ToString());
                    }

                    break;
                }
            }
            else
            {
                var _constraint = node.Constraint;

                foreach (var _parent in parents)
                {
                    if (_parent == _constraint.Name)
                        throw new CircularDependencyException(
                                                              $"{_parent} and {info.Name} have a circular dependency on each other");
                }

                // single operators
                if (SearchForModule(_constraint.Name) == null)
                    _ret = false;

                ModuleInfo _ninfo = null;

                var _loader = new ModuleLoader(SearchPath, Controller);

                try
                {
                    if (!parents.Contains(info.Name))
                        parents.Add(info.Name);

                    _loader.LoadModule(parents, _constraint.Name, out _ninfo, true, true);
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
                                                                              info.Name, _constraint.Name,
                                                                              _constraint.Version, OpToString(_op))
                                                               );
                }

                if (_op == DepOps.Equal || _op == DepOps.GreaterThan || _op == DepOps.GreaterThanEqual ||
                    _op == DepOps.LessThan || _op == DepOps.LessThanEqual || _op == DepOps.NotEqual)
                {
                    if (!IsEmptyVersion(_constraint.Version))
                        if (!VersionMatch(_ninfo.Version, _constraint.Version, _op))
                            if (!opt)
                                throw new UnresolvedDependencyException(
                                                                        string.Format(
                                                                         "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                         info.Name, _constraint.Name,
                                                                         _constraint.Version, OpToString(_op))
                                                                       );

                    if (!checking)
                    {
                        Controller.LoadModule(_constraint.Name);

                        parents.Sort();

                        var _seen = new List<string>
                                    {
                                        info.Name,
                                        _constraint.Name
                                    };
                        foreach (var _p in parents)
                        {
                            if (!_seen.Contains(_p))
                            {
                                Controller.IncRef(_constraint.Name);
                                _seen.Add(_p);
                            }
                        }
                    }
                }

                if (_op == DepOps.Loaded)
                {
                    if (Controller.Loader.SearchForModule(_constraint.Name) == null)
                        if (!opt)
                            throw new UnresolvedDependencyException(
                                                                    string.Format(
                                                                     "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                     info.Name, _constraint.Name,
                                                                     _constraint.Version, OpToString(_op))
                                                                   );

                    if (!checking)
                    {
                        Controller.LoadModule(_constraint.Name);

                        parents.Sort();

                        var _seen = new List<string>
                                    {
                                        info.Name,
                                        _constraint.Name
                                    };
                        foreach (var _p in parents)
                        {
                            if (!_seen.Contains(_p))
                            {
                                Controller.IncRef(_constraint.Name);
                                _seen.Add(_p);
                            }
                        }
                    }
                }

                if (_op == DepOps.NotLoaded)
                    if (Controller.IsLoaded(_constraint.Name))
                    {
                        if (Controller.RefCount(_constraint.Name) > 1)
                            if (!opt)
                                throw new UnresolvedDependencyException(
                                                                        string.Format(
                                                                         "The following dependency for the module {0} could not be resolved: ({3} operator)\n\t{1} ({2})",
                                                                         info.Name, _constraint.Name,
                                                                         _constraint.Version, OpToString(_op))
                                                                       );
                        if (!checking) Controller.UnloadModule(_constraint.Name);
                    }
            }
        }

        /// <summary>
        ///     Converts a given <see cref="DepOps" /> object into a string.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_op">The operator to convert to a string.</param>
        /// <returns>A string value that matches the operator in the <see href="depstring.html">dependency operator table</see>.</returns>
        protected string OpToString(DepOps op)
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
            Version _mver, _drver;

            if (dependencyVersion.Major == -1)
                return true;

            _mver = new Version(dependencyVersion.Major != -1 ? moduleVersion.Major : 0,
                                dependencyVersion.Minor != -1 ? moduleVersion.Minor : 0,
                                dependencyVersion.Build != -1 ? moduleVersion.Build : 0,
                                dependencyVersion.Revision != -1 ? moduleVersion.Revision : 0);

            _drver = new Version(dependencyVersion.Major != -1 ? dependencyVersion.Major : 0,
                                 dependencyVersion.Minor != -1 ? dependencyVersion.Minor : 0,
                                 dependencyVersion.Build != -1 ? dependencyVersion.Build : 0,
                                 dependencyVersion.Revision != -1 ? dependencyVersion.Revision : 0);

            var vres = _mver.CompareTo(_drver);

            return op switch
                   {
                       DepOps.Equal => vres == 0,
                       DepOps.GreaterThan => vres > 0,
                       DepOps.GreaterThanEqual => vres > 0 || vres == 0,
                       DepOps.LessThan => vres < 0,
                       DepOps.LessThanEqual => vres < 0 || vres == 0,
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