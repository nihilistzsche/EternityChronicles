// ECXModuleDLLTask.cs
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
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.BuildTasks;

namespace ECMSBuildTasks
{
    public class ECXModuleDLLTask : ToolTask
    {
        private static readonly string HomePath = Environment.OSVersion.Platform == PlatformID.Unix ||
                                                  Environment.OSVersion.Platform == PlatformID.MacOSX
                                                      ? Environment.GetEnvironmentVariable("HOME")
                                                      : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        protected override string ToolName => "csc.exe";

        protected new string ToolPath =>
            $"{HomePath}/.nuget/packages/microsoft.net.compilers.toolset/4.2.0-1.final/tasks/net472";

        public string WorkingDirectory { get; set; }

        [Required] public ITaskItem[] Sources { get; set; }

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

            foreach (var taskItem in Sources)
            {
                if (!File.Exists(taskItem.ToString()))
                {
                    Log.LogError($"Missing source file {taskItem}.");

                    return false;
                }

                var outputFileName =
                    OutputName ?? Path.GetFileName(taskItem.ToString())
                                      .Replace(".cs", ".dll");

                var cscTask = new Csc
                              {
                                  ToolPath = ToolPath,
                                  ToolExe = ToolName,
                                  TargetType = "library",
                                  AdditionalLibPaths = new[] { LinkDir },
                                  OutputAssembly = new TaskItem(outputFileName),
                                  Sources = Sources,
                                  BuildEngine = BuildEngine
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

                    return false;
                }

                if (OutputDir == null) break;
                var dest = $"{OutputDir}{Path.DirectorySeparatorChar}{outputFileName}";
                if (File.Exists(dest)) File.Delete(dest);

                File.Move(outputFileName, dest);
            }

            return true;
        }
    }
}