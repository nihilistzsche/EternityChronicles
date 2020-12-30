using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EternityChronicles.Glue
{
    public interface ICore
    {
        int Main(bool softReboot, Tuple<SocketInformation, List<SocketInformation>> sockets, params string[] args);

        Tuple<SocketInformation, List<SocketInformation>> DuplicateSockets(int processID);

        int GetProcessID();
    }
}
