﻿// Program.cs
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
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using ECX.Core.Loader;
using EternityChronicles.Glue;

namespace EternityChronicles
{
    internal class Program
    {
        public static ICore Core;

        public static ModuleController Controller;

        private static int Main(string[] args)
        {
            Controller = new ModuleController();

            Controller.RegisterNewRole("Core", typeof(ICore),
                                       (asm, basetype) => { Core = (ICore)asm.CreateInstance(basetype.ToString()); },
                                       asm => { });

            Controller.LoadModule("EternityChronicles.Core");

            Tuple<SocketInformation, List<SocketInformation>> sockets = null;
            int? res;
            while ((res = Core?.Main(sockets != null, sockets, args)) == 9999)
                //// 9999 is soft reboot
                SoftReboot(ref sockets);

            return res ?? 1;
        }

        private static void SoftReboot(ref Tuple<SocketInformation, List<SocketInformation>> sockets)
        {
            sockets = Core.DuplicateSockets(Process.GetCurrentProcess().Id);

            Core = null;

            Controller.UnloadModule("EternityChronicles.Core");

            if (File.Exists("sys/updates/EternityChronicles.Core.dll"))
            {
                File.Copy("sys/updates/EternityChronicles.Core.dll", "./EternityChornicles.Core.dll");
                File.Delete("sys/updates/EternityChronicles.Core.dll");
            }

            Controller.LoadModule("EternityChronicles.Core");
        }

        // For soft-reboot:
        // Retrieve socket information from the app domain and place it in the stub
        // Unload the core app domain
        // look for updated dll in sys/updates/Eternity.Core.dll
        // If found, move it to root directory
        // Try to reload the core app domain
        // If successful, move sockets back into core and stub continues in the background
        // If failure, exit
    }
}