//
//  Copyright 2013  Michael Tindal
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System.Collections.Generic;
using System.Text;

namespace IronDragon.Runtime {
    public partial class DragonInterface : DragonClass {
        public DragonInterface(string name, DragonInterface parent, List<DragonFunction> classMethods,
            List<DragonFunction> instanceMethods) : base(name, parent, classMethods, instanceMethods) {}

        internal DragonInterface() {}

        public override string ToString() {
            var builder = new StringBuilder("DragonInterface: ");
            builder.Append(Name);
            builder.AppendLine(":");
            builder.AppendLine("  Class Methods:");
            foreach (var func in ClassMethods) {
                builder.AppendFormat("    {0}", func);
                builder.AppendLine();
            }
            builder.AppendLine("  Instance Methods:");
            foreach (var func in InstanceMethods) {
                builder.AppendFormat("    {0}", func);
                builder.AppendLine();
            }
            if (Parent != null) {
                builder.AppendLine("Parent: ");
                builder.Append(Parent);
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}