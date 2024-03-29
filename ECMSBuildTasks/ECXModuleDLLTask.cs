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
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.BuildTasks;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ECMSBuildTasks
{
    // ReSharper disable once PartialTypeWithSinglePart
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public partial class ECXModuleDLLTask : ToolTask
    {
        private static readonly string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); 

        protected override string ToolName => "csc.exe";

        protected new string ToolPath =>
            $"{HomePath}{Path.DirectorySeparatorChar}.nuget{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}microsoft.net.compilers.toolset{Path.DirectorySeparatorChar}{CompilerVersion}{Path.DirectorySeparatorChar}tasks{Path.DirectorySeparatorChar}net472";

        public string WorkingDirectory { get; set; }

        [Required] public ITaskItem[] Sources { get; set; }

        public ITaskItem[] Includes { get; set; }

        public string LinkDir { get; set; }

        public string OutputDir { get; set; }

        public string OutputName { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return $"{ToolPath}{Path.DirectorySeparatorChar}{ToolName ?? "csc"}";
        }

        private async Task<bool> CompileAsync(ITaskItem taskItem)
        {
            return await Task.Run(() =>
                                  {
                                      if (!File.Exists(taskItem.ToString()))
                                      {
                                          Log.LogError($"Missing source file {taskItem}.");

                                          return false;
                                      }

                                      var outputFileName = OutputName ?? Path.GetFileName(taskItem.ToString())
                                                                             .Replace(".cs", ".dll");
                                      var fullOutputFileName =
                                          $"{OutputDir}{Path.DirectorySeparatorChar}{outputFileName}";

                                      var cscTask = new Csc
                                                    {
                                                        ToolPath = ToolPath,
                                                        ToolExe = ToolName,
                                                        TargetType = "library",
                                                        AdditionalLibPaths = new[] { LinkDir },
                                                        OutputAssembly = new TaskItem(fullOutputFileName),
                                                        Sources = new[] { taskItem },
                                                        BuildEngine = BuildEngine,
                                                        EchoOff = true
                                                    };
                                      var references = new List<ITaskItem>
                                                       {
                                                           new TaskItem("ECX.Core.dll"),
                                                           new TaskItem("ECX.Core.Module.dll"),
                                                           new TaskItem("EternityChronicles.Tests.dll")
                                                       };
                                      if (Includes != null) references.AddRange(Includes);

                                      cscTask.References = references.ToArray();
                                      return cscTask.Execute();
                                  });
        }

        public override bool Execute()
        {
            Directory.SetCurrentDirectory(WorkingDirectory ?? Directory.GetCurrentDirectory());

            if (Sources != null) return Task.WhenAll(Sources.Select(CompileAsync)).Result.All(x => x);
            Log.LogError("No input sources given.");
            return false;
        }
    }
}