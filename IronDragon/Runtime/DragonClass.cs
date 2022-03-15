// -----------------------------------------------------------------------
// <copyright file="DragonClass.cs" Company="Michael Tindal">
// Copyright 2011-2013 Michael Tindal
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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using IronDragon.Expressions;
using IronDragon.Parser;

namespace IronDragon.Runtime
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public partial class DragonClass
    {
        public DragonClass(string name, DragonClass parent, List<DragonFunction> classMethods,
        List<DragonFunction>      instanceMethods)
        {
            Name         = name;
            ClassMethods = new Dictionary<string, DragonMethodTable>();
            classMethods.ForEach(func => AddMethod(ClassMethods, func));
            if (!ClassMethods.ContainsKey("new"))
                AddMethod(ClassMethods, new DragonFunction("new", new List<FunctionArgument>(),
                DragonExpression.DragonBlock(
                DragonExpression.Return(new List<FunctionArgument>
                {
                    new(null, DragonExpression.Variable(Expression.Constant("self")))
                }),
                Expression.Label(DragonParser.ReturnTarget, Expression.Constant(null, typeof(object)))),
                new DragonScope()));
            InstanceMethods = new Dictionary<string, DragonMethodTable>();
            instanceMethods.ForEach(func => AddMethod(InstanceMethods, func));
            UndefinedMethods = new List<string>();
            RemovedMethods   = new List<string>();
            Context          = new DragonScope();
            Parent           = parent;
        }

        internal DragonClass()
        {
            ClassMethods     = new Dictionary<string, DragonMethodTable>();
            InstanceMethods  = new Dictionary<string, DragonMethodTable>();
            UndefinedMethods = new List<string>();
            RemovedMethods   = new List<string>();
            Context          = new DragonScope();
        }

        public static DragonClass BoxClass(Type type)
        {
            Func<Type, bool> chkDoNotExport = t =>
            {
                var a = t.GetCustomAttributes(typeof(DragonDoNotExportAttribute), false).FirstOrDefault();
                return a != null;
            };
            if (chkDoNotExport(type)) return null;

            if (TypeCache.ContainsKey(type)) return TypeCache[type];

            var @class = type.IsInterface ? new DragonInterface() : (DragonClass)new DragonBoxedClass();
            type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList().ForEach(method => AddMethod(@class.ClassMethods, new DragonNativeFunction(type, method)));
            type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList().ForEach(method => AddMethod(@class.InstanceMethods, new DragonNativeFunction(type, method)));
            type.GetConstructors()
                .ToList().ForEach(ctor => AddMethod(@class.ClassMethods, new DragonNativeFunction(type, ctor)));

            Func<Type, DragonClass> genBaseType = t =>
            {
                if (t.BaseType != null)
                    return TypeCache.ContainsKey(t.BaseType) ? TypeCache[t.BaseType] : BoxClass(t.BaseType);
                return null;
            };

            Func<Type, string> getExportName = t =>
            {
                var a = t.GetCustomAttributes(typeof(DragonExportAttribute), false).FirstOrDefault();
                return a != null ? ((DragonExportAttribute)a).Name : null;
            };

            if (@class is DragonBoxedClass) ((DragonBoxedClass)@class).BoxedType = type;

            @class.Name     = getExportName(type) ?? type.Name;
            @class.Parent   = genBaseType(type);
            TypeCache[type] = @class;
            return @class;
        }

        public static void AddMethod(IDictionary<string, DragonMethodTable> dict, DragonFunction func)
        {
            if (func.Name == "<__doNotExport>") return;
            if (!dict.ContainsKey(func.Name)) dict[func.Name] = new DragonMethodTable(func.Name);


            dict[func.Name].AddFunction(func);
        }

        public void Merge(DragonClass klass)
        {
            foreach (var key in klass.ClassMethods.Keys)
            {
                DragonMethodTable table;
                if (ClassMethods.ContainsKey(key))
                    table = ClassMethods[key];
                else
                    table = new DragonMethodTable(key);
                foreach (var func in klass.ClassMethods[key].Functions) table.AddFunction(func);
            }

            foreach (var key in klass.InstanceMethods.Keys)
            {
                DragonMethodTable table;
                if (InstanceMethods.ContainsKey(key))
                    table = InstanceMethods[key];
                else
                    table = new DragonMethodTable(key);
                foreach (var func in klass.InstanceMethods[key].Functions) table.AddFunction(func);
            }

            Context.MergeWithScope(klass.Context);
        }

        public override string ToString()
        {
            var builder = new StringBuilder("DragonClass: ");
            builder.Append(Name);
            builder.AppendLine(":");
            builder.AppendLine("  Class Methods:");
            foreach (var func in ClassMethods)
            {
                builder.AppendFormat("    {0}", func);
                builder.AppendLine();
            }

            builder.AppendLine("  Instance Methods:");
            foreach (var func in InstanceMethods)
            {
                builder.AppendFormat("    {0}", func);
                builder.AppendLine();
            }

            if (Parent != null)
            {
                builder.AppendLine("Parent: ");
                builder.Append(Parent);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        #region Properties

        internal static readonly Dictionary<Type, DragonClass> TypeCache = new();

        public Dictionary<string, DragonMethodTable> ClassMethods { get; }

        public Dictionary<string, DragonMethodTable> InstanceMethods { get; }

        public List<string> UndefinedMethods { get; }

        public List<string> RemovedMethods { get; }

        public DragonScope Context { get; internal set; }

        public string Name { get; internal set; }

        public DragonClass Parent { get; internal set; }

        #endregion
    }
}