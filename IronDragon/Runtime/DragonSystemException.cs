// DragonSystemException.cs in EternityChronicles/IronDragon
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