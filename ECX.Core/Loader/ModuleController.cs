// ModuleController.cs in EternityChronicles/ECX.Core
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
using System.Runtime.InteropServices;
using ECX.Core.Dependency;
using ECX.Core.Module;

namespace ECX.Core.Loader
{
    /// <summary>
    ///     A ModuleController object is the bridge between an application
    ///     and its modules.  The controller manages the loading of modules, registration
    ///     of roles, and other behind the scenes information needed for the operation of
    ///     the module engine.
    /// </summary>
    /// <remarks>None.</remarks>
    /// <preliminary />
    public class ModuleController
    {
        /// <summary>
        ///     Creates a new ModuleController
        ///     object and initializes the members.
        /// </summary>
        /// <remarks>None.</remarks>
        public ModuleController()
        {
            AppDomainMap = new Dictionary<string, AppDomain>();
            AppDomainAssemblyMap = new Dictionary<Assembly, AppDomain>();
            RefCounts = new Dictionary<AppDomain, int>();
            SearchPath = new List<string>();
            Roles = new List<ModuleRole>();
            Loader = new ModuleLoader(SearchPath, this);
            InfoMap = new Dictionary<string, ModuleInfo>();
        }

        #region Members

        /// <summary>
        ///     This maintains a map of module names to their
        ///     <see cref="System.AppDomain">AppDomain</see>.
        /// </summary>
        /// <remarks>None.</remarks>
        protected Dictionary<string, AppDomain> AppDomainMap;

        /// <summary>
        ///     A map of assemblies to their containing AppDomains.
        /// </summary>
        protected Dictionary<Assembly, AppDomain> AppDomainAssemblyMap;

        /// <summary>
        ///     Maintains a map of AppDomains to their
        ///     reference counts.
        /// </summary>
        /// <remarks>None.</remarks>
        protected Dictionary<AppDomain, int> RefCounts;

        /// <summary>
        ///     The list of directories used to search for
        ///     modules.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <summary>
        ///     A list of registered roles.
        /// </summary>
        /// <remarks>None.</remarks>
        protected List<ModuleRole> Roles;

        /// <summary>
        ///     Maintains a map of module names
        ///     to their <see cref="ModuleInfo" />
        ///     object.
        /// </summary>
        /// <remarks>None.</remarks>
        protected Dictionary<string, ModuleInfo> InfoMap;

        public delegate void ModuleLoadedHandler(ModuleInfo sender, EventArgs e);

        public event ModuleLoadedHandler ModuleLoaded;

        #endregion

        #region Loading/Unloading

        public void LoadAllModules(List<string> modules)
        {
            foreach (var module in modules) LoadModule(module);
        }

        /// <summary>
        ///     Loads a module with the given name.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_name">The name of the module minus the .dll extension.</param>
        public void LoadModule(string name)
        {
            LoadModule(null, name);
        }

        /// <summary>
        ///     Loads a module with the given name and parents.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="parents">The parent modules for the loaded module.</param>
        /// <param name="name">The name of the module to load minus the .dll extension.</param>
        public void LoadModule(List<string> parents, string name)
        {
            if (name == null) return;

            if (AppDomainMap.ContainsKey(name))
            {
                IncRef(AppDomainMap[name]);
                return; // Already loaded, no need to load it again.
            }


            var _domain = Loader.LoadModule(parents, name, out var _info, false, true);

            // set up the map
            AppDomainMap.Add(name, _domain);
            AppDomainAssemblyMap.Add(Loader.GetAssembly(_domain, name), _domain);

            // increment the reference count on the domain.
            IncRef(_domain);

            // increment the reference count for all the dependencies recursively (i.e.
            // if module A depends on B which depends on C, B gets inc ref'd once, while C
            // gets inc ref'd twice, for both A and B).

            // Entry handlers
            foreach (var asm in _domain.GetAssemblies()) CallEntryHandler(asm);

            // Set up roles.
            CallRoleHandlers(_info);

            // Set up info map.
            InfoMap.Add(name, _info);

            ModuleLoaded?.Invoke(_info, new EventArgs());
        }

        /// <summary>
        ///     Checks to see whether a module is loaded or not.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="name">The name of the module to check.</param>
        /// <returns>Returns true if the module is loaded, false otherwise.</returns>
        public bool IsLoaded(string name)
        {
            return AppDomainMap.ContainsKey(name);
        }

        public ModuleInfo GetInfoForAssembly(Assembly asm)
        {
            return InfoMap.Values.FirstOrDefault(info => info.Owner == asm);
        }

        public AppDomain GetAppDomainForAssembly(Assembly asm)
        {
            return AppDomainAssemblyMap[asm];
        }

        public Assembly GetAssemblyForModule(string name)
        {
            return InfoMap[name]?.Owner;
        }

        /// <summary>
        ///     Recursively decrements reference counts on modules
        ///     listed in the given dependency tree.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_x">The root of the tree to recurse.</param>
        protected void DecRefs(DepNode node)
        {
            if (node == null)
                return;

            foreach (var child in node.Children) DecRefs(child);

            DecRef(AppDomainMap[node.Constraint.Name]);
        }

        /// <summary>
        ///     Unloads a module.
        /// </summary>
        /// <remarks>None.</remarks>
        /// <param name="_name">The name of the module to unload.</param>
        /// <exception cref="DomainStillReferencedException">Thrown when the domain holding the module is still referenced.</exception>
        public void UnloadModule(string name)
        {
            // This is fun stuff.  We can't unload a module any of the following conditions fail:
            //  1) The module must be a top-level node in the dep map, i.e no other modules can
            //  be depending on it.
            //  2) The reference count on the appdomain must be 1, which means the only thing
            //  using this appdomain is the module inside of it.

            if (!AppDomainMap.ContainsKey(name))
                return; // suckers not loaded, why are we unloading it?

            var info = InfoMap[name];

            var domain = AppDomainMap[name];


            if (RefCounts[domain] > 1)
                throw new DomainStillReferencedException(
                                                         $"The domain holding the module {name} cannot be unloaded because it is still being referenced.");

            // okay, everything's good.  This will remove the domain from the reference list since its reference count is now 0.
            DecRef(domain);

            var _root = info.Dependencies;

            DecRefs(_root);

            // okay, lets remove the domain map association
            AppDomainMap.Remove(name);

            // Also need to remove the assembly map
            AppDomainAssemblyMap.Remove(Loader.GetAssembly(domain, name));

            // the info map needs to go too
            InfoMap.Remove(name);

            // Let people know theyre module has been unloaded.
            CallRoleUnregisterHandlers(info);

            // Exit handlers
            CallExitHandler(domain.GetAssemblies()[0]);

            // And finally, unload the domain.
            AppDomain.Unload(domain);
        }

        #endregion

        #region Domain Reference Counts

        /// <summary>
        ///     Increments the reference count on a given module.
        /// </summary>
        /// <remarks>
        ///     See <see href="refcount.html">Reference Counts</see> for an explanation of reference counts.
        /// </remarks>
        /// <param name="_name">The name of the module to incremement the reference count on.</param>
        public void IncRef(string name)
        {
            IncRef(AppDomainMap[name]);
        }

        /// <summary>
        ///     Increments the reference count on a given domain.
        /// </summary>
        /// <remarks>
        ///     See <see href="refcount.html">Reference Counts</see> for an explanation of reference counts.
        /// </remarks>
        /// <param name="_domain">The domain to increment the reference count on.</param>
        protected void IncRef(AppDomain domain)
        {
            if (!RefCounts.ContainsKey(domain)) RefCounts.Add(domain, 0);

            RefCounts[domain]++;
        }

        /// <summary>
        ///     Decrements the reference count on a given module.
        /// </summary>
        /// <remarks>
        ///     See <see href="refcount.html">Reference Counts</see> for an explanation of reference counts.
        /// </remarks>
        /// <param name="_name">The name of the module to decremement the reference count on.</param>
        public void DecRef(string name)
        {
            DecRef(AppDomainMap[name]);
        }

        /// <summary>
        ///     Decrements the reference count on a given domain.
        /// </summary>
        /// <remarks>
        ///     See <see href="refcount.html">Reference Counts</see> for an explanation of reference counts.
        /// </remarks>
        /// <param name="_domain">The domain to decrement the reference count on.</param>
        protected void DecRef(AppDomain domain)
        {
            if (!RefCounts.ContainsKey(domain))
                return;

            RefCounts[domain] = RefCounts[domain]--;

            if (RefCounts[domain] == 0) RefCounts.Remove(domain); // no references, this suckers getting unloaded.
        }

        #endregion

        #region Role registration

        /// <summary>
        ///     Registers a new role.
        /// </summary>
        /// <remarks>
        ///     Registering a new role will go through already loaded modules to see if any of them
        ///     fulfill the new role.  See <see href="roles.html">Roles</see> for an explanation of module roles.
        /// </remarks>
        /// <param name="_name">The name of the new role.</param>
        /// <param name="_type">The base type of the new role.</param>
        /// <param name="_reg">The registration handler for the new role.</param>
        /// <param name="_unreg">The unregistration handler for the new role.</param>
        public void RegisterNewRole(string name, Type type, RoleRegisterHandler reg, RoleUnregisterHandler unreg)
        {
            var _role = new ModuleRole(name, type, reg, unreg);

            Roles.Add(_role);

            foreach (var _key in InfoMap.Keys) CallRoleHandlers(InfoMap[_key], _role);
        }

        /// <summary>
        ///     Unregisters a role.
        /// </summary>
        /// <remarks>
        ///     See <see href="roles.html">Roles</see> for an explanation of module roles.
        /// </remarks>
        /// <param name="_name">The name of the role to unregister.</param>
        public void UnregisterRole(string name)
        {
            ModuleRole _role = null;
            foreach (var _r in Roles)
            {
                if (_r.RoleName == name)
                    _role = _r;
            }

            if (_role == null)
                return;

            Roles.Remove(_role);
            foreach (var _key in InfoMap.Keys) CallRoleUnregisterHandlers(InfoMap[_key], _role);
        }

        #endregion

        #region Role Handlers

        /// <summary>
        ///     Finds a list of modules along the search path that could fulfill a given role
        /// </summary>
        public List<string> SearchForModulesForRole(string roleName)
        {
            var ret = new List<string>();

            var role = (from p in Roles where p.RoleName == roleName select p).FirstOrDefault();

            if (role == null) return ret;

            var modules = from s in SearchPath
                          where Directory.Exists(s)
                          from f in Directory.GetFiles(s, "*.dll")
                          select f;

            foreach (var module in modules)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(module);
                    Loader.LoadModule(null, name, out var info, true, true);
                    if (info.Roles != null && info.Roles.Contains(roleName)) ret.Add(name);
                }
                catch (Exception)
                {
                    ;
                }
            }

            ret.Sort();
            return ret;
        }

        protected bool DoesAssemblyFulfillRole(Assembly assembly, ModuleRole role, out Type type)
        {
            foreach (var _type in assembly.GetTypes())
            {
                if (role.BaseType.IsClass)
                {
                    if (_type.IsSubclassOf(role.BaseType))
                    {
                        type = _type;
                        return true;
                    }
                }
                else if (role.BaseType.IsInterface)
                {
                    if (_type.GetInterfaces().Contains(role.BaseType))
                    {
                        type = _type;
                        return true;
                    }
                }
            }

            type = null;
            return false;
        }

        /// <summary>
        ///     Calls all of the role registration handlers for the given
        ///     <see cref="ModuleInfo" /> object.
        /// </summary>
        /// <remarks>
        ///     See <see href="roles.html">Roles</see> for an explanation of module roles.
        /// </remarks>
        /// <param name="_info">The <see cref="ModuleInfo" /> object to determine which role handlers to call.</param>
        protected void CallRoleHandlers(ModuleInfo info)
        {
            foreach (var _role in Roles) CallRoleHandlers(info, _role);
        }

        /// <summary>
        ///     Calls the role registration handler for the given role if the
        ///     given <see cref="ModuleInfo" /> object fulfills the role.
        /// </summary>
        /// <remarks>
        ///     See <see href="roles.html">Roles</see> for an explanation of module roles.
        /// </remarks>
        /// <param name="_info">The <see cref="ModuleInfo" /> object to determine if the role handler should be called.</param>
        /// <param name="_role">The role to fulfill.</param>
        protected void CallRoleHandlers(ModuleInfo info, ModuleRole _role)
        {
            if (info.Roles == null)
                return;

            foreach (var _myRole in info.Roles.Split(','))
            {
                if (_role.RoleName == _myRole)
                {
                    var _asm = info.Owner;


                    if (!DoesAssemblyFulfillRole(_asm, _role, out var _type))
                        continue; // don't have a type for this role.

                    _role.RegistrationHandler(_asm, _type);
                }
            }
        }

        /// <summary>
        ///     Calls all of the role unregistration handlers for the given
        ///     <see cref="ModuleInfo" /> object.
        /// </summary>
        /// <remarks>
        ///     See <see href="roles.html">Roles</see> for an explanation of module roles.
        /// </remarks>
        /// <param name="_info">The <see cref="ModuleInfo" /> object to determine which role handlers to call.</param>
        protected void CallRoleUnregisterHandlers(ModuleInfo info)
        {
            foreach (var _role in Roles) CallRoleUnregisterHandlers(info, _role);
        }

        /// <summary>
        ///     Calls the role unregistration handler for the given role if the
        ///     given <see cref="ModuleInfo" /> object fulfills the role.
        /// </summary>
        /// <remarks>
        ///     See <see href="roles.html">Roles</see> for an explanation of module roles.
        /// </remarks>
        /// <param name="_info">The <see cref="ModuleInfo" /> object to determine if the role handler should be called.</param>
        /// <param name="_role">The role to fulfill.</param>
        protected void CallRoleUnregisterHandlers(ModuleInfo info, ModuleRole role)
        {
            if (info.Roles != null)
                return;

            foreach (var _myRole in info.Roles.Split(','))
            {
                if (role.RoleName == _myRole)
                {
                    var _asm = info.Owner;


                    if (!DoesAssemblyFulfillRole(_asm, role, out var _type))
                        continue; // don't have a type for this role.

                    role.UnregistrationHandler(_asm);
                }
            }
        }

        #endregion

        #region Entry/Exit Handlers

        /// <summary>
        ///     Calls the modules entry handler.
        /// </summary>
        /// <remarks>
        ///     See <see href="exhandler.html">Entry/Exit Handlers</see> for an explanation of handlers.
        /// </remarks>
        /// <param name="_asm">The assembly to call the entry handler for.</param>
        protected void CallEntryHandler(_Assembly assembly)
        {
            foreach (var _type in assembly.GetTypes())
            {
                if (_type.GetInterface(typeof(IModule).ToString()) != null)
                {
                    var _method = _type.GetMethod("ModuleEntry");

                    if (_method != null)
                        _method.Invoke(assembly.CreateInstance(_type.ToString()), new object[] { this });
                }
            }
        }

        /// <summary>
        ///     Calls the modules exit handler.
        /// </summary>
        /// <remarks>
        ///     See <see href="exhandler.html">Entry/Exit Handlers</see> for an explanation of handlers.
        /// </remarks>
        /// <param name="_asm">The assembly to call the exit handler for.</param>
        protected void CallExitHandler(_Assembly assembly)
        {
            foreach (var _type in assembly.GetTypes())
            {
                if (_type.GetInterface(typeof(IModule).ToString()) != null)
                {
                    var _method = _type.GetMethod("ModuleExit");

                    if (_method != null)
                        _method.Invoke(assembly.CreateInstance(_type.ToString()), new object[] { this });
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the search path for this controller.
        /// </summary>
        /// <remarks>
        ///     See <see href="searching.html">this</see> for information on search paths.
        /// </remarks>
        public List<string> SearchPath { get; set; }

        /// <summary>
        ///     Gets the module loader used by this controller.
        /// </summary>
        /// <remarks>None.</remarks>
        public ModuleLoader Loader { get; }

        /// <summary>
        ///     Gets the reference count on the given module.
        /// </summary>
        /// <remarks>
        ///     See <see href="refcount.html">Reference Counts</see> for an explanation of reference counts.
        /// </remarks>
        /// <param name="_name">The name of the module to check.</param>
        /// <returns>The reference count on the given module.</returns>
        public int RefCount(string name)
        {
            if (!AppDomainMap.ContainsKey(name))
                return -1;

            return RefCounts[AppDomainMap[name]];
        }

        #endregion
    }
}