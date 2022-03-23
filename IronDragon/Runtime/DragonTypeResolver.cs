// DragonTypeResolver.cs in EternityChronicles/IronDragon
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
using System.Linq;
using Theraot.Collections;

namespace IronDragon.Runtime
{
    public static class DragonTypeResolver
    {
        private static readonly List<string> _includedNamespaces = new();

        static DragonTypeResolver()
        {
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