using System;
using System.Linq;
using IronDragon;
using IronDragon.Runtime;

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
                    var source = engine.CreateScriptSourceFromFile(arg);
                    source.Execute();
                }

                return;
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
                try
                {
                    var result = code.Execute(scope);
                    Console.WriteLine(result);
                    ContextRootScope.MergeWithScope(scope);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}