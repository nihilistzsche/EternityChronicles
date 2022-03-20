// DragonSystemException.cs in EternityChronicles/IronDragon
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

namespace IronDragon.Runtime
{
    // Used to throw a Dragon-based exception class
    [DragonDoNotExport]
    public class DragonSystemException : Exception
    {
        public DragonSystemException(DragonInstance obj)
        {
            var klass = obj.Class;

            var exceptionFound = false;
            var _class = obj.Class;
            do
            {
                if (_class.Name.Equals("Exception"))
                {
                    exceptionFound = true;
                    break;
                }

                _class = _class.Parent;
            } while (!exceptionFound && _class != null);

            if (exceptionFound)
            {
                ExceptionClass = klass;
                InnerObject = obj;
            }
            else
            {
                ExceptionClass = Dragon.Box(typeof(DragonSystemException));
                InnerObject = null;
            }
        }

        public DragonInstance InnerObject { get; }

        public DragonClass ExceptionClass { get; }
    }
}