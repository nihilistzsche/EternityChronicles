// DragonTypeResolver.cs in EternityChronicles/IronDragon
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
using Theraot.Collections;

namespace IronDragon.Runtime
{
    public static class DragonTypeResolver
    {
        private static readonly List<string> _includedNamespaces = new();

        static DragonTypeResolver()
        {
            _includedNamespaces.Add(""); // So you can specify the namespace yourself
            var namespaces = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes()).Flatten()
                                      .Select(t => t.Namespace).Distinct();
            _includedNamespaces.AddRange(namespaces);
        }

        public static List<string> IncludedNamespaces => _includedNamespaces;

        public static void Include(string @namespace)
        {
            _includedNamespaces.Add(@namespace);
        }

        public static Type Resolve(string name)
        {
            var mq =
                _includedNamespaces.Where(
                                          @namespace =>
                                              Type.GetType(string.Format("{0}.{1}", @namespace, name)) != null);
            return mq.Any() ? Type.GetType($"{mq.First()}.{name}") : null;
        }
    }
}