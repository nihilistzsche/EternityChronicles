using System;
using System.Net;
using System.Net.Sockets;
using DragonMUD.Utility;
using System.Collections.Generic;

namespace DragonMUD.Network
{
    public class Server : BaseObject, IGameLoopObject
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Server _defaultServer;

        public bool SoftReboot { get; private set; }

        static Server()
        {
            _defaultServer = new Server();
        }

        public static Server DefaultServer()
        {
            return _defaultServer;
        }

        public Server()
        {
            ConnectionPool = new ConnectionPool();
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
                {
                    foreach(var clientSocket in softRebootSockets.Item2)
                    {
                        var socket = new Socket(clientSocket);
                        ConnectionPool.NewConnection(socket, true);
                    }
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

        public void OnGameLoop()
        {
            
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

        public Socket Socket { get; private set;  }

        public ConnectionPool ConnectionPool { get; }

        public bool IsRunning { get; private set;  }
    }
}
