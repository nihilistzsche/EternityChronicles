using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonMUD.Interpreters.Logic
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        
        public List<string> Aliases { get; }
        
        public int RequiredLevel { get; }
        
        public List<string> Flags { get; }

        public string ShortHelp { get; }
        
        public string LongHelp { get; }
        
        public CommandAttribute(string name, string aliases, int requiredLevel, string flags, string shortHelp = "", string longHelp = "")
        {
            Name = name;
            Aliases = aliases.Split(' ').ToList();
            RequiredLevel = requiredLevel;
            Flags = flags.Split(' ').ToList();
            ShortHelp = shortHelp;
            LongHelp = longHelp;
        }
    }
}