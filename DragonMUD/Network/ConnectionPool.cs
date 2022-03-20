// ConnectionPool.cs in EternityChronicles/DragonMUD
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