using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
namespace ECMSBuildTasks
{
    public class ECXModuleDLLTask : ToolTask
    {
        protected override string ToolName => "csc";

        protected new string ToolPath => "/usr/bin";

        public string WorkingDirectory { get; set;  }

        public ITaskItem[] Sources { get; set; }

        public ITaskItem[] Includes { get; set; }

        public string LinkDir { get; set; }

        public string OutputDir { get; set; }

        public string OutputName { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolName);
        }

        public override bool Execute()
        {
            Directory.SetCurrentDirectory(WorkingDirectory ?? Directory.GetCurrentDirectory());

            if (Sources == null)
            {
                Log.LogError("No input sources given.");
                return false;
            }

            foreach (var taskItem in Sources)
            {
                if (!File.Exists(taskItem.ToString()))
                {
                    Log.LogError($"Missing source file {taskItem}.");
                    return false;
                }

                var outputFileName = OutputName ?? Path.GetFileName(taskItem.ToString()).Replace(".cs", ".dll");

                var cscTask = new Csc
                {
                    TargetType = "library",
                    AdditionalLibPaths = new string[] { LinkDir },
                    OutputAssembly = new TaskItem(outputFileName),
                    Sources = Sources,
                    BuildEngine = BuildEngine

                };
                var references = new List<ITaskItem>
                {
                    new TaskItem("ECX.Core.dll"), new TaskItem("ECX.Core.Module.dll"),
                    new TaskItem("EternityChronicles.Tests.dll")

                };
                if (Includes != null)
                {
                    references.AddRange(Includes);
                }

                cscTask.References = references.ToArray();
                Log.LogMessage(MessageImportance.High, $"{taskItem} => {outputFileName}");
                var result = cscTask.Execute();
                if (!result)
                {
                    Log.LogError($"Error while compiling source file {taskItem}.");
                    return false;
                }

                if (OutputDir == null) continue;
                var dest = $"{OutputDir}{Path.DirectorySeparatorChar}{outputFileName}";
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }

                File.Move(outputFileName, dest);
            }

            return true;
        }
    }
}