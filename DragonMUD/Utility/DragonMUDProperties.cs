// DragonMUDProperties.cs
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

namespace DragonMUD.Utility
{
    public class DragonMUDProperties
    {
        public readonly Dictionary<string, dynamic> Properties = new();

        public dynamic this[string propertyPath]
        {
            get
            {
                if (propertyPath == string.Empty)
                    return null;

                var propertyNames = propertyPath.Split('.').ToList();

                if (propertyNames.Count() == 1)
                    return Properties[propertyNames[0]];

                var dict = GetPropertyDictFromName(propertyNames[0]);

                Properties[propertyNames[0]] = dict;

                propertyNames.RemoveAt(0);

                propertyPath = string.Join(".", propertyNames);

                return dict[propertyPath];
            }
            set
            {
                if (propertyPath == string.Empty)
                    return;

                var propertyNames = propertyPath.Split('.').ToList();

                if (propertyNames.Count() == 1)
                    Properties[propertyNames[0]] = value;

                var dict = GetPropertyDictFromName(propertyNames[0]);

                Properties[propertyNames[0]] = dict;

                propertyNames.RemoveAt(0);

                propertyPath = string.Join(".", propertyNames);

                dict[propertyPath] = value;
            }
        }

        private DragonMUDProperties GetPropertyDictFromName(string name)
        {
            return Properties[name] as DragonMUDProperties ?? new DragonMUDProperties();
        }
    }
}