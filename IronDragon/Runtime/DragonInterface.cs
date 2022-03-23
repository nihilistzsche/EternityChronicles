// DragonInterface.cs in EternityChronicles/IronDragon
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
using System.Text;

namespace IronDragon.Runtime
{
    public partial class DragonInterface : DragonClass
    {
        public DragonInterface(string name, DragonInterface parent, List<DragonFunction> classMethods,
                               List<DragonFunction> instanceMethods) : base(name, parent, classMethods, instanceMethods)
        {
        }

        internal DragonInterface()
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder("DragonInterface: ");
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
    }
}