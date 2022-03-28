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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Markup;
using System.Xml.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.BuildTasks;

namespace ECMSBuildTasks
{
    public partial class ECXModuleDLLTask : ToolTask
    {
        private static readonly string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); 

        protected override string ToolName => "csc.exe";

        protected new string ToolPath =>
            $"{HomePath}{Path.DirectorySeparatorChar}.nuget{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}microsoft.net.compilers.toolset{Path.DirectorySeparatorChar}{CompilerVersion}{Path.DirectorySeparatorChar}tasks{Path.DirectorySeparatorChar}net472";

        private string CachePath => $"{HomePath}{Path.DirectorySeparatorChar}.cache{Path.DirectorySeparatorChar}ecxmoduledllcache";

        [Serializable]
        public class TaskCache
        {
            public List<byte[]> FileCaches { get; } = new();
        }
        
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

        public byte[] CalcSHA512ForTaskItem(ITaskItem taskItem)
        {
            var filename = File.Exists(taskItem.ItemSpec) ? taskItem.ItemSpec : $"{LinkDir}{Path.DirectorySeparatorChar}{taskItem.ItemSpec}";
            if (!File.Exists(filename))
            {
                return new byte[] { };
            }
            var file = File.ReadAllText(filename);
            var byteArray = Encoding.UTF8.GetBytes(file);
            var sha512 = new SHA512Managed();
            return sha512.ComputeHash(byteArray);
        }

        public bool CheckCompilationNecessary(TaskCache cache, ITaskItem taskItem, Csc compiler)
        {
            var needsCompilation = false;
            var validCache = cache.FileCaches.Count > 1;
            var oldHash = validCache ? cache.FileCaches[0] : new byte[] {};
            var hash = CalcSHA512ForTaskItem(taskItem);
            if (hash != oldHash)
            {
                if (validCache)
                    cache.FileCaches[0] = hash;
                else
                    cache.FileCaches.Add(hash);

                needsCompilation = true;
            }

            Debug.Assert(compiler.References != null, "compiler.References != null");
            var i = 1;
            foreach (var reference in compiler.References)
            {
                var refOldHash = validCache ? cache.FileCaches[i++] : new byte[] {};
                var refHash = CalcSHA512ForTaskItem(reference);
                if (refHash == refOldHash) continue;
                if (!validCache)
                {
                    cache.FileCaches.Add(refHash);
                }
                else
                {
                    cache.FileCaches[i-1] = refHash;
                }

                needsCompilation = true;
            }

            return needsCompilation;
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
                if (!Directory.Exists(CachePath))
                {
                    Directory.CreateDirectory(CachePath);
                }

                TaskCache cacheObj = null;
                var cacheName =
                    $"{CachePath}{Path.DirectorySeparatorChar}.{taskItem.ItemSpec.Substring(taskItem.ItemSpec.LastIndexOf(Path.DirectorySeparatorChar) + 1)}.cache";
                if (File.Exists(cacheName))
                {
                    cacheObj =
                        (new XmlSerializer(typeof(TaskCache))
                                .Deserialize(new FileStream(cacheName, FileMode.Open))) as
                        TaskCache;
                    
                }

                cacheObj = cacheObj ?? new TaskCache();
                if (!CheckCompilationNecessary(cacheObj, taskItem, cscTask))
                {
                    Log.LogMessage("Compilation for {taskItem} is unnecessary, skipping.");
                    return true;
                }
                
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
                if (File.Exists(cacheName))
                {
                    File.Delete(cacheName);
                }

                Log.LogMessage("Serializing?");
                new XmlSerializer(typeof(TaskCache))
                    .Serialize(new FileStream(cacheName, FileMode.Create), cacheObj);
            }

            return true;
        }
    }
}