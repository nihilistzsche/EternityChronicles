using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.BuildTasks;

namespace ECMSBuildTasks
{
    public class ECXModuleDLLTask : ToolTask
    {
        private static readonly string HomePath = Environment.OSVersion.Platform == PlatformID.Unix ||
            Environment.OSVersion.Platform                                       == PlatformID.MacOSX
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        protected override string ToolName => "csc.exe";

        protected new string ToolPath =>
            $"{HomePath}/.nuget/packages/microsoft.net.compilers.toolset/4.2.0-1.final/tasks/net472";

        public string WorkingDirectory { get; set; }

        public ITaskItem[] Sources { get; set; }

        public ITaskItem[] Includes { get; set; }

        public string LinkDir { get; set; }

        public string OutputDir { get; set; }

        public string OutputName { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolName ?? "csc");
        }

        public override bool Execute()
        {
            Directory.SetCurrentDirectory(WorkingDirectory ?? Directory.GetCurrentDirectory());

            if (Sources == null)
            {
                Log.LogError("No input sources given.");
                return false;
            }

            var retValue = true;
            Parallel.ForEach(Sources, taskItem =>
            {
                if (!File.Exists(taskItem.ToString()))
                {
                    Log.LogError($"Missing source file {taskItem}.");
                    if (retValue) retValue = false;

                    return;
                }

                var outputFileName =
                    OutputName ?? Path.GetFileName(taskItem.ToString())
                        .Replace(".cs", ".dll");

                var cscTask = new Csc
                {
                    ToolPath           = ToolPath,
                    ToolExe            = ToolName,
                    TargetType         = "library",
                    AdditionalLibPaths = new[] { LinkDir },
                    OutputAssembly     = new TaskItem(outputFileName),
                    Sources            = Sources,
                    BuildEngine        = BuildEngine
                };
                var references = new List<ITaskItem>
                {
                    new TaskItem("ECX.Core.dll"),
                    new TaskItem("ECX.Core.Module.dll"),
                    new TaskItem("EternityChronicles.Tests.dll")
                };
                if (Includes != null) references.AddRange(Includes);

                cscTask.References = references.ToArray();
                Log.LogMessage(MessageImportance.High, $"{taskItem} => {outputFileName}");
                var result = cscTask.Execute();
                if (!result)
                {
                    Log.LogError($"Error while compiling source file {taskItem}.");
                    if (retValue) retValue = false;

                    return;
                }

                if (OutputDir == null) return;
                var dest = $"{OutputDir}{Path.DirectorySeparatorChar}{outputFileName}";
                if (File.Exists(dest)) File.Delete(dest);

                File.Move(outputFileName, dest);
            });

            return retValue;
        }
    }
}