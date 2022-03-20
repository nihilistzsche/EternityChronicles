// DragonScope.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronDragon.Builtins;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace IronDragon.Runtime
{
    public static class DictionaryExtensions
    {
        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example:
        //   result = map.MergeLeft(other1, other2, ...)
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            var newMap = new T();
            foreach (var src in
                     new List<IDictionary<K, V>> { me }.Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (var p in src) newMap[p.Key] = p.Value;
            }

            return newMap;
        }
    }

    /// <summary>
    ///     This class is the scope we use for managing variables.
    /// </summary>
    public class DragonScope
    {
        internal static readonly Dictionary<string, Symbol> Symbols = new();

        private static int _staticScopeId;

        private readonly int _scopeId = _staticScopeId++;

        public DragonScope(DragonScope parent = null)
        {
            Variables = new Dictionary<string, dynamic>();
            SymVars = new Dictionary<Symbol, dynamic>();
            Aliases = new Dictionary<string, string>();
            Constants = new List<string>();
            if (parent != null)
                ParentScope = parent;
        }

        public DragonScope(ScriptScope scope)
            : this()
        {
            MergeWithScope(scope);
        }

        internal DragonScope(List<string> constants) : this()
        {
            Constants = constants;
        }

        internal DragonScope RootScope => ParentScope == null ? this : ParentScope.RootScope;

        internal List<string> Constants { get; }

        internal Dictionary<string, dynamic> Variables { get; private set; }

        internal Dictionary<Symbol, dynamic> SymVars { get; private set; }

        internal Dictionary<string, string> Aliases { get; }

        public dynamic this[string name]
        {
            get => Resolve(name);
            set
            {
                if (CheckConstant(name))
                    throw new ConstantException(
                                                string
                                                    .Format("{0} is already defined as a constant in this scope or a parent scope.",
                                                            name));
                var val = value;
                if (val is string) val = new DragonString(val);
                if (DragonNumber.IsConvertable(val)) val = new DragonNumber(val);
                Variables[name] = val;
            }
        }

        public dynamic this[Symbol sym]
        {
            get => Resolve(sym);
            set
            {
                var val = value;
                if (val is string) val = new DragonString(val);
                if (DragonNumber.IsConvertable(val)) val = new DragonNumber(val);
                SymVars[sym] = val;
            }
        }

        public virtual DragonScope ParentScope { get; set; }

        public void MergeWithScope(Scope scope)
        {
            List<KeyValuePair<string, dynamic>> items = scope.Storage.GetItems();

            foreach (var item in items)
            {
                if (!Variables.ContainsKey(item.Key))
                {
                    var val = item.Value;
                    // Runtime classes we should wrap to get desired effect
                    if (val is DragonArray || val is DragonDictionary || val is DragonString || val is DragonNumber)
                        val = Dragon.Box(val);
                    Variables[item.Key] = val;
                }
            }
        }

        public void MergeWithScope(ScriptScope scope)
        {
            scope.GetVariableNames().ToList().ForEach(name =>
                                                      {
                                                          var val = scope.GetVariable(name);
                                                          // Runtime classes we should wrap to get desired effect
                                                          if (val is DragonArray || val is DragonDictionary ||
                                                              val is DragonString || val is DragonNumber)
                                                              val = Dragon.Box(val);
                                                          Variables[name] = val;
                                                      });
        }

        public void MergeWithScope(DragonScope other)
        {
            Variables = other.Variables.MergeLeft(Variables);
            SymVars = other.SymVars.MergeLeft(SymVars);
        }

        public void MergeIntoScope(Scope scope)
        {
            // Exit scope
            foreach (var var in Variables)
            {
                var val = var.Value;
                if (val is DragonBoxedInstance bo) val = bo.BoxedObject;

                scope.Storage[var.Key] = val;
            }
        }

        public void MergeIntoScope(ScriptScope scope)
        {
            foreach (var var in Variables)
            {
                var val = var.Value;
                if (val is DragonBoxedInstance bo) val = bo.BoxedObject;

                scope.SetVariable(var.Key, val);
            }
        }

        protected virtual bool CheckAliases(string name)
        {
            if (Aliases.ContainsKey(name)) return true;
            return ParentScope?.CheckAliases(name) ?? Dragon.Globals.CheckAliases(name);
        }

        protected virtual bool CheckConstant(string name)
        {
            if (Constants.Contains(name)) return true;
            return ParentScope?.CheckConstant(name) ?? Dragon.Globals.CheckConstant(name);
        }

        protected virtual DragonScope GetAlias(string name)
        {
            if (Aliases.ContainsKey(name)) return this;
            if (ParentScope != null) return ParentScope.GetAlias(name);
            return Dragon.Globals.GetAlias(name);
        }

        internal void AddAlias(string from, string to)
        {
            if (GetAlias(to) != null || CheckConstant(to) || GetAlias(from) != null || Resolve(from) == null ||
                Variables.ContainsKey(to))
                return;
            Aliases[to] = from;
            Constants.Add(to);
        }

        protected virtual dynamic Resolve(string name)
        {
            if (CheckAliases(name))
            {
                var scope = GetAlias(name);
                return scope.Resolve(scope.Aliases[name]);
            }

            if (Variables.ContainsKey(name)) return Variables[name];
            if (ParentScope != null) return ParentScope.Resolve(name);
            return Dragon.Globals.Resolve(name);
        }

        protected virtual dynamic Resolve(Symbol sym)
        {
            if (SymVars.ContainsKey(sym)) return SymVars[sym];
            var val = ParentScope != null ? ParentScope.Resolve(sym) : Dragon.Globals.Resolve(sym);
            return val ?? Resolve(sym.Name);
        }

        public void PrintVariables()
        {
            PrintVariables(false);
        }

        private string WasParentString(bool wasParent, int depth = 0)
        {
            if (wasParent)
            {
                var sb = new StringBuilder("");
                for (var i = 0; i < depth; i++) sb.Append("  ");
                return sb.ToString();
            }

            return "";
        }

        private void PrintVariables(bool wasParent, int depth = 0)
        {
#if DEBUG
            if (!wasParent) Console.WriteLine("---BEGIN---");
            Console.WriteLine("{0}Scope {1}", WasParentString(wasParent, depth), _scopeId);
            foreach (var p in Variables)
                Console.WriteLine("{0}{1} => {2}", WasParentString(wasParent, depth + 1), p.Key, p.Value);
            if (ParentScope != null)
                ParentScope.PrintVariables(true, depth + 1);
            else
                Console.WriteLine("---END---");
#endif
        }

        internal DragonScope SearchForObject(object obj, out string name)
        {
            foreach (var p in Variables)
            {
                if ((object)p.Value == obj && p.Key != "self" && p.Key != "super")
                {
                    name = p.Key;
                    return this;
                }
            }

            if (ParentScope != null) return ParentScope.SearchForObject(obj, out name);
            name = null;
            return null;
        }

        public override string ToString()
        {
            return string.Format("[Scope {0}]", _scopeId);
        }

        public static implicit operator Scope(DragonScope @this)
        {
            var scope = new Scope();
            @this.MergeIntoScope(scope);
            return scope;
        }

        public static implicit operator ScriptScope(DragonScope @this)
        {
            var engine = Dragon.CreateRuntime().GetEngine("IronDragon");
            var scope = engine.CreateScope();

            foreach (var var in @this.Variables) scope.SetVariable(var.Key, var.Value);

            return scope;
        }

        public dynamic GetVariable(string varName)
        {
            return DragonScriptCode.Convert(Resolve(varName), this);
        }

        public void RemoveVariable(string varName)
        {
            if (Variables.ContainsKey(varName)) Variables.Remove(varName);
        }
    }
}