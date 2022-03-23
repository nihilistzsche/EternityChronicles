// VariableManager.cs in EternityChronicles/XDL
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
using System.IO;
using System.Linq;
using System.Text;
using CSLog;

namespace XDL
{
    public class VariableManager
    {
        static VariableManager()
        {
            Log.RegisterChannel("xdl");
            DefaultManager = new VariableManager();
        }

        public VariableManager(string fileName = null)
        {
            FileName = fileName;
            LoadVariables();
        }

        public string FileName { get; set; }

        public Dictionary<string, string> Variables { get; } = new();

        public static VariableManager DefaultManager { get; }

        public void LoadVariables()
        {
            if (FileName == null) return;
            Log.LogMessage("xdl", LogLevel.Info, $"Reading configuration file {FileName}...");

            var lines = File.ReadAllLines(FileName);

            foreach (var parts in lines.Where(line => line.Length > 0 && line[0] != '#').Select(line => line.Split('='))
                                       .Where(parts => parts.Count() > 1))
            {
                var name = parts[0];
                var value = parts[1];

                if (value[value.Length - 1] == ';') value = value.Substring(0, value.Length - 1);

                DefaultManager.Variables[name] = value;
                Variables[name] = value;
                Log.LogMessage("xdl", LogLevel.Info, $"Set variable {name} to value {value}.");
            }
        }

        public bool SaveVariables()
        {
            if (FileName == null)
                return false;

            using (var fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                foreach (var bytes in (from key in Variables.Keys
                                       let value = Variables[key]
                                       select $"{key}={value};{Environment.NewLine}"
                                      ).Select(varString => Encoding.UTF8.GetBytes(varString))
                        )
                    fs.Write(bytes, 0, bytes.Length);
            }

            return true;
        }
    }

    public static class StringExtensions
    {
        public static string ReplaceAllVariables(this string @this, VariableManager manager = null)
        {
            manager ??= VariableManager.DefaultManager;

            string current;
            do
            {
                current = string.Copy(@this);
                foreach (var key in manager.Variables.Keys)
                {
                    var str = $"$({key})";
                    @this = @this.Replace(str, manager.Variables[key]);
                }
            } while (current != @this);

            return @this;
        }
    }
}