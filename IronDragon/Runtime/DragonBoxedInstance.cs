// DragonBoxedInstance.cs in EternityChronicles/IronDragon
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
        internal static void SyncInstanceVariablesFrom(DragonInstance DragonObject, object obj)
        {
            var fields =
                obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            DragonObject.InstanceVariables.Variables.Keys.ToList().ForEach(key =>
                                                                           {
                                                                               var fq = fields
                                                                                   .Where(field => field.Name == key)
                                                                                   .ToList();
                                                                               if (!fq.Any()) return;
                                                                               var val =
                                                                                   DragonObject.InstanceVariables[key];
                                                                               if (val is DragonNumber)
                                                                                   val = DragonNumber.Convert(val);
                                                                               fq.First().SetValue(obj, val);
                                                                           });
        }

        // .net -> Dragon
        internal static void SyncInstanceVariablesTo(DragonInstance DragonObject, object obj)
        {
            var fields =
                obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            fields.ForEach(field => DragonObject.InstanceVariables[field.Name] = field.GetValue(obj));
        }
    }
}