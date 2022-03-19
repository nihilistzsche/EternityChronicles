using System;
using System.Linq;
using IronDragon;
using IronDragon.Runtime;
using static System.IO.Path;

namespace IDragon
{
    internal static class Program
    {
        private static readonly DragonScope ContextRootScope = new DragonScope();

        public static void Main(string[] args)
        {
            var runtime = Dragon.CreateRuntime();
            var engine = runtime.GetEngine("IronDragon");

            if (args.Any())
            {
                foreach (var arg in args)
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
                    source.Execute();

                    return;
                }
            }

            if (Console.IsInputRedirected)
            {
                var stdin = Console.In.ReadToEnd();
                var source = engine.CreateScriptSourceFromString(stdin);
                source.Execute();
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
    }
}