// DragonBoxedInstance.cs in EternityChronicles/IronDragon
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronDragon.Builtins;

namespace IronDragon.Runtime
{
    public class DragonBoxedInstance : DragonInstance
    {
        private static readonly Dictionary<object, DragonBoxedInstance> BoxCache =
            new();

        internal DragonBoxedInstance(object obj, DragonScope scope, DragonClass @class) : base(@class)
        {
            BoxedObject = obj;
            BoxedScope = scope;
        }

        protected DragonBoxedInstance(object obj, DragonScope scope) : base(GetBoxClass(obj))
        {
            BoxedObject = obj;
            BoxedScope = scope;
        }

        internal object BoxedObject { get; }

        internal DragonScope BoxedScope { get; }

        public static DragonBoxedInstance Box(object obj, DragonScope scope = null)
        {
            if (obj == null) return null;
            if (BoxCache.ContainsKey(obj))
            {
                BoxCache[obj].BoxedScope.MergeWithScope(scope ?? new DragonScope());
                return BoxCache[obj];
            }

            var boxed = new DragonBoxedInstance(obj, scope ?? new DragonScope());
            BoxCache[obj] = boxed;
            if (scope == null) return boxed;
            var objScope = scope.SearchForObject(obj, out var name);
            if (objScope != null) objScope[name] = boxed;
            return boxed;
        }

        public static DragonBoxedInstance BoxNoCache(object obj, DragonScope scope = null)
        {
            if (obj == null) return null;
            var boxed = new DragonBoxedInstance(obj, scope ?? new DragonScope());
            if (scope == null) return boxed;
            var objScope = scope.SearchForObject(obj, out var name);
            if (objScope != null) objScope[name] = boxed;
            return boxed;
        }

        public static dynamic Unbox(DragonBoxedInstance obj)
        {
            BoxCache.Remove(obj.BoxedObject);
            return obj.BoxedObject;
        }

        private static DragonClass GetBoxClass(object obj)
        {
            return Dragon.Box(obj.GetType());
        }

        // Dragon -> .net
        internal static void SyncInstanceVariablesFrom(DragonInstance dragonObject, object obj)
        {
            var fields =
                obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            dragonObject.InstanceVariables.Variables.Keys.ToList().ForEach(key =>
                                                                           {
                                                                               var fq = fields
                                                                                   .Where(field => field.Name == key)
                                                                                   .ToList();
                                                                               if (!fq.Any()) return;
                                                                               var val =
                                                                                   dragonObject.InstanceVariables[key];
                                                                               if (val is DragonNumber)
                                                                                   val = DragonNumber.Convert(val);
                                                                               fq.First().SetValue(obj, val);
                                                                           });
        }

        // .net -> Dragon
        internal static void SyncInstanceVariablesTo(DragonInstance dragonObject, object obj)
        {
            var fields =
                obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            fields.ForEach(field => dragonObject.InstanceVariables[field.Name] = field.GetValue(obj));
        }
    }
}