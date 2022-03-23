// ECXModuleDLLTask.cs in EternityChronicles/ECMSBuildTasks
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