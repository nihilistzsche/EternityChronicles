﻿// ConnectionCoordinator.cs in EternityChronicles/DragonMUD
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
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using CSLog;

namespace DragonMUD.Network
{
    public interface IOutputHook
    {
        string Process(ConnectionCoordinator coordinator);
    }

    public interface IWriteHook
    {
        string ProcessMessage(string message);
    }

    public class InputEventArgs : EventArgs
    {
        public InputEventArgs(string input)
        {
            Input = input;
        }

        public string Input { get; }
    }

    public delegate void InputReceivedHandler(ConnectionCoordinator sender, InputEventArgs e);

    public class ConnectionCoordinator : BaseObject
    {
        private const int InputBufferSize = 1024;

        private readonly StringBuilder _inputBuffer = new();
        private readonly byte[] _inputBufferRaw = new byte[InputBufferSize];

        public ConnectionCoordinator(Socket socket, ConnectionPool owner)
        {
            Owner = owner;
            Socket = socket;

            if (socket.Connected)
            {
                Incoming = new Subject<string>();
                Outgoing = new Subject<string>();

                Incoming.Subscribe(s => ReadData(this, s));
                Outgoing.Subscribe(s => Send(this, s));

                socket.BeginReceive(_inputBufferRaw, 0, InputBufferSize, 0, BeginRead, this);
            }
            else
            {
                owner.RemoveConnection(this);
            }
        }

        public Socket Socket { get; }

        private ISubject<string> Incoming { get; }

        private ISubject<string> Outgoing { get; }

        private Dictionary<string, IOutputHook> OutputHooks { get; } = new();

        private ConnectionPool Owner { get; }

        public event InputReceivedHandler InputReceived;

        public void SendMessage(string message, params object[] pObjects)
        {
            var msg = string.Format(message, pObjects);

            msg = Owner.WriteHooks.Aggregate(msg, (current, hook) => hook.ProcessMessage(current));

            Outgoing.OnNext(msg);
        }

        private void Send(ConnectionCoordinator owner, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            owner.Socket.BeginSend(bytes, 0, bytes.Length, 0, SendCallback, owner);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var coordinator = (ConnectionCoordinator)ar.AsyncState;

                coordinator.Socket.EndSend(ar);
                if (!IsFlagSet("no-message"))
                {
                    // Get current state, SendStateMessage(coordinator);
                }

                ClearFlag("no-message");

                foreach (var hook in OutputHooks.Select(outputHook => outputHook.Value)) hook.Process(coordinator);
            }
            catch (Exception)
            {
                Owner.RemoveConnection(this);
            }
        }

        public void Disconnect()
        {
            Owner.RemoveConnection(this);
        }

        private void BeginRead(IAsyncResult ar)
        {
            try
            {
                var owner = (ConnectionCoordinator)ar.AsyncState;
                var bytes = owner.Socket.EndReceive(ar);

                var str = Encoding.UTF8.GetString(owner._inputBufferRaw, 0, bytes);
                Incoming.OnNext(str);
            }
            catch (Exception)
            {
                Owner.RemoveConnection(this);
            }
        }

        private void ReadData(ConnectionCoordinator owner, string inputString)
        {
            var socket = owner.Socket;

            if (inputString.Length == 0 || inputString[0] == '\x04')
            {
                Log.LogMessage("dragonmud", LogLevel.Info,
                               $"Encountered end-of-file from socket {socket.Handle.ToInt32()}, closing connection...");
                owner.Owner.RemoveConnection(this);
                return;
            }

            inputString = inputString.Replace("\0", string.Empty);

            owner._inputBuffer.Append(inputString);

            if (!owner._inputBuffer.ToString().EndsWith("\n", StringComparison.InvariantCulture)) return;
            var inputStringF = owner._inputBuffer.ToString();
            inputStringF = inputStringF.Replace("\r", string.Empty);
            inputStringF = inputStringF.Replace("\n", string.Empty);

            owner.InputReceived?.Invoke(owner, new InputEventArgs(inputStringF));

            owner._inputBuffer.Clear();

            owner.Socket.BeginReceive(owner._inputBufferRaw, 0, InputBufferSize, 0, BeginRead, owner);
        }
    }
}