// ConnectionPool.cs in EternityChronicles/DragonMUD
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
using System.Net.Sockets;
using DragonMUD.StateMachine;
using XDL;

namespace DragonMUD.Network
{
    public class ConnectionPool : BaseObject
    {
        public List<ConnectionCoordinator> Connections { get; } = new();

        public List<IWriteHook> WriteHooks { get; } = new();

        public string Greeting { get; set; } = null;

        // public IState DefaultState { get; set; } = null;

        public ConnectionCoordinator NewConnection(Socket socket, bool softReboot = false, string name = null)
        {
            var greeting = Greeting ?? $"Welcome to $(Name).{Environment.NewLine}Please enter your account name:";

            var coordinator = new ConnectionCoordinator(socket, this);
            if (name != null && !softReboot)
                coordinator["name"] = name;
            // load xml from tmp with state
            Connections.Add(coordinator);

            if (!softReboot)
            {
                coordinator.SendMessage(greeting.ReplaceAllVariables());
            }
            else
            {
                //state stuff woo 
                var state = coordinator["current-state"] as IState;
                state?.SendSoftRebootMessage(coordinator);
            }

            return coordinator;
        }

        public void WriteToAllConnections(string message)
        {
            foreach (var coordinator in Connections) coordinator.SendMessage(message);
        }

        public static void WriteToConnections(List<ConnectionCoordinator> connections, string message)
        {
            foreach (var coordinator in connections) coordinator.SendMessage(message);
        }

        public void RemoveConnection(ConnectionCoordinator coordinator)
        {
            if (!Connections.Contains(coordinator))
                return;

            coordinator.Socket.Close();
            Connections.Remove(coordinator);
        }
    }
}