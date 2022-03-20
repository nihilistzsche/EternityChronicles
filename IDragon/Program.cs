using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;
using IronDragon;
using IronDragon.Runtime;
using static System.IO.Path;

namespace IDragon
{
    internal static class Program
    {
        private static readonly DragonScope ContextRootScope = new DragonScope();

        private static void ParseOptions(Options o)
        {
            if (o.Version)
            {
                Console.WriteLine("IDragon -- The IrongDragon Interactive Interpreter.");
                Console.WriteLine("Component versions:");
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).ToList();
                var idragonAssembly = assemblies.First(x => x.Name == "IDragon");
                var irondragonAssembly = assemblies.First(x => x.Name == "IronDragon");
                assemblies.Remove(idragonAssembly);
                assemblies.Remove(irondragonAssembly);
                Console.WriteLine($"IDragon: Version {idragonAssembly.Version}");
                Console.WriteLine($"IronDragon: Version {irondragonAssembly.Version}");
                Console.WriteLine(" ");
                Console.WriteLine("Other loaded assemblies:");
                foreach (var asmName in assemblies)
                {
                    Console.WriteLine($"{asmName.Name} Version: {asmName.Version}");
                }

                return;
            }

            var runtime = Dragon.CreateRuntime();
            var engine = runtime.GetEngine("IronDragon");

            foreach (var arg in o.FileNames)
            {
                if (arg.Contains(DirectorySeparatorChar))
                {
                    var parts = arg.Split(DirectorySeparatorChar);
                    Dragon.SetCurrentDirectory(string.Join(DirectorySeparatorChar.ToString(),
                                                           parts.Take(parts.Length - 1)));
                }
                else
                {
                    Dragon.SetCurrentDirectory(null);
                }

                var source = engine.CreateScriptSourceFromFile(arg);
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
                Parser.Default.ParseArguments<Options>(args).WithParsed(ParseOptions);
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
            public Options(bool version, IEnumerable<string> fileNames)
            {
                Version = version;
                FileNames = fileNames;
            }

            [Option('v', "version", Required = false, HelpText = "Shows the version info and exits.")]
            public bool Version { get; }

            [Value(0, Required = false, MetaName = "Input files",
                   HelpText = "[file1.dragon] [file2.dragon] ... [fileN.dragon]")]
            public IEnumerable<string> FileNames { get; }
        }
    }
}