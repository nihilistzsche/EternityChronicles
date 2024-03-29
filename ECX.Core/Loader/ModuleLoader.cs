// ModuleLoader.cs in EternityChronicles/ECX.Core
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
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using ECX.Core.Dependency.Resolver;
using ECX.Core.Module;

namespace ECX.Core.Loader
{
    /// <summary>
    ///     This class loads modules, resolves dependencies, and other
    ///     low-level functions.
    /// </summary>
    /// <remarks>None.</remarks>
    /// <preliminary />
    public class ModuleLoader
    {
        private static string _sCurrentDllPath;

        /// <summary>
        ///     The controller which this loader belongs to.
        /// </summary>
        /// <remarks>None.</remarks>
        protected ModuleController Controller;

        /// <summary>
        ///     The module search path.
        /// </summary>
        /// <remarks>
        ///     See <see href="searching.html">this</see> for information on search paths.
        /// </remarks>
        protected List<string> SearchPath;

        static ModuleLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        /// <summary>
        ///     Creates a new ModuleLoader object with the given search path
        ///     and <see cref="ModuleController" /> object.
        /// </summary>
        /// <remarks>None.</remarks>
        public ModuleLoader(List<string> searchPath, ModuleController controller)
        {
            SearchPath = searchPath;
            Controller = controller;
        }

        /// <summary>
        ///     Loads the contents of a module into a byte array.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="filename">The name of the file to load.</param>
        /// <returns>Byte array containing the contents of the file.</returns>
        protected byte[] LoadRawFile(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        /// <summary>
        ///     Searchs for a module along the search path.
        /// </summary>
        /// <remarks>
        ///     See <see href="searching.html">this</see> for information on search paths.
        /// </remarks>
        /// <param name="_name">The name of the module to search for.</param>
        /// <returns>The full path to the module if found, otherwise null.</returns>
        public string SearchForModule(string name)
        {
            var res = (from s in SearchPath
                       where Directory.Exists(s)
                       from f in Directory.GetFiles(s, "*.dll")
                       let g = f.Replace(s, "").Replace(Path.DirectorySeparatorChar.ToString(), "")
                       where g[..^4] == name
                       select s + Path.DirectorySeparatorChar + g).FirstOrDefault() ??
                      (from h in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll")
                       let j =
                           h.Replace(Directory.GetCurrentDirectory(), "")
                            .Replace(Path.DirectorySeparatorChar.ToString(), "")
                       where j[..^4] == name
                       select Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + j).FirstOrDefault();

            return res;
        }

        /// <summary>
        ///     Loads a module.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_name">The name of the module to load.</param>
        /// <param name="_info">A <see cref="ECX.Core.Module.ModuleInfo" /> object to hold the loaded module's information.</param>
        /// <returns>A <see cref="System.AppDomain" /> object containing the loaded module.</returns>
        /// <exception cref="ModuleNotFoundException">Thrown if the module was not found along the search path.</exception>
        /// <exception cref="ModuleImageException">Thrown if the module image is not a valid assembly.</exception>
        /// <exception cref="InvalidModuleException">Thrown if the assembly is not a valid ECX module.</exception>
        public AppDomain LoadModule(string name, out ModuleInfo info)
        {
            return LoadModule(null, name, out info, false);
        }

        /// <summary>
        ///     Loads a module.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_parents">A list of the modules parents.</param>
        /// <param name="_name">The name of the module to load.</param>
        /// <param name="_info">A <see cref="ECX.Core.Module.ModuleInfo" /> object to hold the loaded module's information.</param>
        /// <param name="checking">If true nothing is loaded, if false the module is loaded.</param>
        /// <returns>A <see cref="System.AppDomain" /> object containing the loaded module.</returns>
        /// <exception cref="ModuleNotFoundException">Thrown if the module was not found along the search path.</exception>
        /// <exception cref="ModuleImageException">Thrown if the module image is not a valid assembly.</exception>
        /// <exception cref="InvalidModuleException">Thrown if the assembly is not a valid ECX module.</exception>
        public AppDomain LoadModule(List<string> parents, string name, out ModuleInfo info, bool checking)
        {
            return LoadModule(parents, name, out info, checking, true);
        }

        /// <summary>
        ///     Gets the assembly representing the module from a <see cref="System.AppDomain" /> object.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_domain">The domain to search.</param>
        /// <param name="_name">The name of the module to get.</param>
        /// <returns>An assembly object with the same name or null.</returns>
        public Assembly GetAssembly(_AppDomain domain, string name)
        {
            return domain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == name);
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            using var fs = new FileStream(_sCurrentDllPath, FileMode.Open);
            var bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            return Assembly.Load(bytes);
        }

        /// <summary>
        ///     Loads a module.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_parents">A list of the modules parents.</param>
        /// <param name="_name">The name of the module to load.</param>
        /// <param name="_info">A <see cref="ECX.Core.Module.ModuleInfo" /> object to hold the loaded module's information.</param>
        /// <param name="checking">If true nothing is loaded, if false the module is loaded.</param>
        /// <param name="depcheck">If set to true dependencies are checked, if false dependencies are ignored.</param>
        /// <returns>A <see cref="System.AppDomain" /> object containing the loaded module.</returns>
        /// <exception cref="ModuleNotFoundException">Thrown if the module was not found along the search path.</exception>
        /// <exception cref="ModuleImageException">Thrown if the module image is not a valid assembly.</exception>
        /// <exception cref="InvalidModuleException">Thrown if the assembly is not a valid ECX module.</exception>
        public AppDomain LoadModule(List<string> parents, string name, out ModuleInfo info, bool checking,
                                    bool depcheck)
        {
            // Okay, this is tricky.  First, we have to load the module into a temp domain
            // to retrieve its module info.  Then, we have to attempt to resolve the dependencies.
            // This is going to be fun.  Heh.
            if (parents == null)
                parents = new List<string>();

            // Try to find the module on the search path.

            var filename = SearchForModule(name);

            if (filename == null)
                throw new ModuleNotFoundException($"The module {name} was not found along the module search path.");

            // Okay, well, now we know the module exists at least in the file (we hope its a proper dll, but we'll see :).  Now we
            // need to create the temporary AppDomain and load it to get the info from it.
            var setup = new AppDomainSetup();

            var sb = new StringBuilder();
            var sep = "";
            foreach (var sp in SearchPath)
            {
                sb.AppendFormat("{0}{1}", sep, sp.Replace('/', Path.DirectorySeparatorChar));
                sep = ";";
            }

            setup.ApplicationBase = Directory.GetCurrentDirectory();
            setup.PrivateBinPath = sb.ToString();
            var tempDomain = AppDomain.CreateDomain(name, new Evidence(), setup);
            _sCurrentDllPath = filename.Replace('/', Path.DirectorySeparatorChar);

            try
            {
                tempDomain.Load(LoadRawFile(filename));
            }
            catch (BadImageFormatException e)
            {
                AppDomain.Unload(tempDomain);
                throw new ModuleImageException(e.Message);
            }

            // Okay, now lets grab the module info from the assembly attributes.

            var asm = GetAssembly(tempDomain, name);

            info = new ModuleInfo(asm);

            // okay, now we've got the info, let's do some magic with the dependencies.
            // this will recursively load all of the appropriate assemblies as per the parsed
            // depedency tree.  It will take into account dependency operators, such as AND, OR
            // OPT (optional).  Very intelligent stuff.  Of course, if there are no depends,
            // this just simply returns.  This will of course continue updating the parents as needed
            // since each time a new module is loaded, the resolver is recursively called until
            // a module with no dependencies is found.  This is cool.  What this will do is call this method with
            // checking=true, which will cause it to just return if the module suceeds.  This way
            // we can ensure we don't load unneeded module Z that is a dependency of X which depends
            // on Y, because if Z suceeds but Y fails, we don't want X, Y, or Z to fail.  This way,
            // we can ensure the entire tree can be loaded first (this does take into account already
            // loaded assemblies).

            if (depcheck)
            {
                var resolver = new DepResolver(Controller, SearchPath);

                parents.Add(name);

                try
                {
                    resolver.Resolve(parents, info);
                }
                catch (Exception)
                {
                    AppDomain.Unload(tempDomain);
                    throw;
                }
            }

            if (!checking) return tempDomain;
            AppDomain.Unload(tempDomain);
            return null;
        }
    }
}