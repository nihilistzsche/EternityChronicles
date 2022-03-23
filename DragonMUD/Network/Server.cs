// Server.cs in EternityChronicles/DragonMUD
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
using System.Net;
using System.Net.Sockets;
using DragonMUD.Utility;

namespace DragonMUD.Network
{
    public class Server : BaseObject, IGameLoopObject
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Server _defaultServer;

        static Server()
        {
            _defaultServer = new Server();
        }

        public Server()
        {
            ConnectionPool = new ConnectionPool();
        }

        public bool SoftReboot { get; private set; }

        public Socket Socket { get; private set; }

        public ConnectionPool ConnectionPool { get; }

        public bool IsRunning { get; private set; }

        public void OnGameLoop()
        {
        }

        public static Server DefaultServer()
        {
            return _defaultServer;
        }

        public bool StartServer(int port, Tuple<SocketInformation, List<SocketInformation>> softRebootSockets = null)
        {
            var endPoint = new IPEndPoint(IPAddress.IPv6Any, port);

            try
            {
                if (softRebootSockets != null)
                {
                    Socket = new Socket(softRebootSockets.Item1);
                }
                else
                {
                    Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                    Socket.Bind(endPoint);
                }

                IsRunning = true;
                if (softRebootSockets != null)
                    foreach (var clientSocket in softRebootSockets.Item2)
                    {
                        var socket = new Socket(clientSocket);
                        ConnectionPool.NewConnection(socket, true);
                    }

                Socket.Listen(100);
                Socket.BeginAccept(BeginAccept, Socket);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void BeginAccept(IAsyncResult ar)
        {
            var socket = Socket.EndAccept(ar);
            ConnectionPool.NewConnection(socket);

            Socket.BeginAccept(BeginAccept, Socket);
        }

        public void Shutdown()
        {
            Socket.Close();
            IsRunning = false;
        }

        public void StartSoftReboot()
        {
            IsRunning = false;
            SoftReboot = true;
        }
    }
}