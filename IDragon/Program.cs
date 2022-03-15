using System;
using IronDragon;
using IronDragon.Runtime;

namespace IDragon
{
    internal static class Program
    {
        private static readonly DragonScope ContextScope = new DragonScope();

        public static void Main(string[] args)
        {
            var runtime = Dragon.CreateRuntime();
            var engine  = runtime.GetEngine("IronDragon");

            while (true)
            {
                Console.Write("Dragon> ");
                var line = Console.In.ReadLine();
                if (line == "exit" || line == "quit") break;

                var scope = engine.CreateScope();
                ContextScope.MergeIntoScope(scope);
                var code   = engine.CreateScriptSourceFromString(line);
                var result = code.Execute(scope);
                Console.WriteLine(result);
                ContextScope.MergeWithScope(scope);
            }
        }
    }
}