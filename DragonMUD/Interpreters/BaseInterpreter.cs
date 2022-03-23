// BaseInterpreter.cs in EternityChronicles/DragonMUD
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

using DragonMUD.Network;

namespace DragonMUD.Interpreters
{
    public abstract class BaseInterpreter
    {
        public abstract void Interpret(ConnectionCoordinator coordinator, InputEventArgs input);

        public void AttachToCoordinator(ConnectionCoordinator coordinator)
        {
            coordinator.InputReceived += Interpret;
        }

        public void DetatchFromCoordinator(ConnectionCoordinator coordinator)
        {
            coordinator.InputReceived -= Interpret;
        }
    }

    public static class InterpreterHelper
    {
        private static BaseInterpreter GetInterpreterForCoordinator(ConnectionCoordinator coordinator)
        {
            return coordinator["current-interpreter"] as BaseInterpreter;
        }

        private static void SetInterpreterForCoordinator(ConnectionCoordinator coordinator, BaseInterpreter interpreter)
        {
            GetInterpreterForCoordinator(coordinator)?.DetatchFromCoordinator(coordinator);

            coordinator["current-interpreter"] = interpreter;

            interpreter.AttachToCoordinator(coordinator);
        }
    }
}