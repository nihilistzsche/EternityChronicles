// DragonMUDProperties.cs in EternityChronicles/DragonMUD
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

namespace DragonMUD.Utility
{
    public class DragonMudProperties
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

        private DragonMudProperties GetPropertyDictFromName(string name)
        {
            return Properties[name] as DragonMudProperties ?? new DragonMudProperties();
        }
    }
}