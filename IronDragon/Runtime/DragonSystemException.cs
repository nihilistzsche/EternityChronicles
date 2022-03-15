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
            var _class         = obj.Class;
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
                InnerObject    = obj;
            }
            else
            {
                ExceptionClass = Dragon.Box(typeof(DragonSystemException));
                InnerObject    = null;
            }
        }

        public DragonInstance InnerObject { get; }

        public DragonClass ExceptionClass { get; }
    }
}