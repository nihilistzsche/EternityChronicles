// Program.cs in EternityChronicles/IDragon
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
using CommandLine;
using IronDragon;
using IronDragon.Runtime;
using static System.IO.Path;

namespace IDragon
{
    internal static class Program
    {
        private static readonly DragonScope ContextRootScope = new();

        private static void ParseOptions(Options o)
        {
            if (o.Version)
            {
                Console.WriteLine("IDragon -- The IronDragon Interactive Interpreter.");
                Console.WriteLine("Component versions:");
                var assemblies = (from x in AppDomain.CurrentDomain.GetAssemblies() select x.GetName()).ToList();
                var idragonAssembly = assemblies.First(x => x.Name == "IDragon");
                var irondragonAssembly = assemblies.First(x => x.Name == "IronDragon");
                assemblies.Remove(idragonAssembly);
                assemblies.Remove(irondragonAssembly);
                Console.WriteLine($"IDragon: Version {idragonAssembly.Version}");
                Console.WriteLine($"IronDragon: Version {irondragonAssembly.Version}");
                Console.WriteLine(" ");
                Console.WriteLine("Other loaded assemblies:");
                foreach (var asmName in assemblies) Console.WriteLine($"{asmName.Name} Version: {asmName.Version}");

                return;
            }

            if (o.Help)
            {
                Console.WriteLine("IDragon.exe usage information:");
                Console.WriteLine(" ");
                Console.WriteLine(" -v | --version: Shows the version information and exits.");
                Console.WriteLine(" -h | --help: Shows this help information and exits.");
                Console.WriteLine(" ");
                Console.WriteLine(" [file1.dragon] [file2.dragon] ... [fileN.dragon]: Execute the given script files with the IronDragon engine.");
                Console.WriteLine(" ");
                Console.WriteLine(" ");
                Console.WriteLine("Without any arguments, IDragon.exe opens into an interactive shell.");
                Console.WriteLine(" ");
                Console.WriteLine("If standard input is redirected, for example by piping in data, the piped data is loaded as a script and executed.");
            }

            var runtime = Dragon.CreateRuntime();
            var engine = runtime.GetEngine("IronDragon");

            foreach (var filename in o.FileNames)
            {
                if (filename.Contains(DirectorySeparatorChar))
                {
                    var parts = filename.Split(DirectorySeparatorChar);
                    Dragon.SetCurrentDirectory(string.Join(DirectorySeparatorChar.ToString(),
                                                           parts.Take(parts.Length - 1)));
                }
                else
                {
                    Dragon.SetCurrentDirectory(null);
                }

                var source = engine.CreateScriptSourceFromFile(filename);
                var scope = engine.CreateScope();
                ContextRootScope.MergeIntoScope(scope);
                source.Execute(scope);
                ContextRootScope.MergeWithScope(scope);
            }
        }

        public static void Main(string[] args)
        {
            var runtime = Dragon.CreateRuntime();
            var engine = runtime.GetEngine("IronDragon");

            if (Console.IsInputRedirected)
            {
                var stdin = Console.In.ReadToEnd();
                var source = engine.CreateScriptSourceFromString(stdin);
                source.Execute();
                return;
            }

            if (args.Any())
            {
                new Parser(config =>
                           {
                               config.HelpWriter = null;
                               config.AutoHelp = false;
                               config.AutoVersion = false;
                               config.EnableDashDash = true;
                           }).ParseArguments<Options>(args).WithParsed(ParseOptions);
                return;
            }

            while (true)
            {
                Console.Write("Dragon> ");
                var line = Console.In.ReadLine();
                if (line != null && (line.StartsWith("exit") || line.StartsWith("quit"))) break;

                var scope = engine.CreateScope();
                ContextRootScope.MergeIntoScope(scope);
                var code = engine.CreateScriptSourceFromString(line);
                var result = code.Execute(scope);
                if (result != null) Console.WriteLine(result);

                ContextRootScope.MergeWithScope(scope);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class Options
        {
            public Options(bool version, bool help, IEnumerable<string> fileNames)
            {
                Version = version;
                Help = help;
                FileNames = fileNames;
            }

            [Option('v', "version", Required = false, HelpText = "Shows the version info and exits.")]
            public bool Version { get; }

            [Option('h', "help", Required = false, HelpText = "Shows the help information")]
            public bool Help { get; }

            [Value(0, Required = false, MetaName = "Input files",
                   HelpText = "[file1.dragon] [file2.dragon] ... [fileN.dragon]")]
            public IEnumerable<string> FileNames { get; }
        }
    }
}