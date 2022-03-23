// CommandAttribute.cs in EternityChronicles/DragonMUD
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

namespace DragonMUD.Interpreters.Logic
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name, string aliases, int requiredLevel, string flags, string shortHelp = "",
                                string longHelp = "")
        {
            Name = name;
            Aliases = aliases.Split(' ').ToList();
            RequiredLevel = requiredLevel;
            Flags = flags.Split(' ').ToList();
            ShortHelp = shortHelp;
            LongHelp = longHelp;
        }

        public string Name { get; }

        public List<string> Aliases { get; }

        public int RequiredLevel { get; }

        public List<string> Flags { get; }

        public string ShortHelp { get; }

        public string LongHelp { get; }
    }
}