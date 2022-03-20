// CommandAttribute.cs
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