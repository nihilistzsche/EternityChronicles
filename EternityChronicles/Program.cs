// Program.cs in EternityChronicles/EternityChronicles
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
                                       asm => { Core = null; });

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
            sockets = Core.DuplicateSockets(Core.GetProcessId());

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