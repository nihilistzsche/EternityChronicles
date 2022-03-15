using System.Collections.Generic;

namespace IronDragon.Runtime
{
    public class DragonModule
    {
        public DragonModule(string name, DragonScope context, List<object> contents)
        {
            Name     = name;
            Contents = contents;
            Context  = context;
        }

        internal DragonModule()
        {
            Contents = new List<object>();
        }

        public string Name { get; internal set; }

        public List<object> Contents { get; }

        public DragonScope Context { get; internal set; }
    }
}