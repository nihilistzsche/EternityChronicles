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
        private static readonly DragonScope ContextRootScope = new DragonScope();

        private static void ParseOptions(Options o)
        {
            if (o.Version) Console.WriteLine("IDragon 0.9: Interactive IronDragon interpreter.");

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

        private sealed class Options
        {
            [Option('v', "version", Required = false, HelpText = "Shows the version info and exits.")]
            public bool Version { get; set; }

            [Value(0, Required = false, MetaName = "Input files",
                   HelpText = "[file1.dragon] [file2.dragon] ... [fileN.dragon]")]
            public IEnumerable<string> FileNames { get; set; }
        }
    }
}