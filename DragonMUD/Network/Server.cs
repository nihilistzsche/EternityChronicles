// Server.cs
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