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
        static BaseInterpreter GetInterpreterForCoordinator(ConnectionCoordinator coordinator)
        {
            return coordinator["current-interpreter"] as BaseInterpreter;
        }

        static void SetInterpreterForCoordinator(ConnectionCoordinator coordinator, BaseInterpreter interpreter)
        {
            GetInterpreterForCoordinator(coordinator)?.DetatchFromCoordinator(coordinator);

            coordinator["current-interpreter"] = interpreter;

            interpreter.AttachToCoordinator(coordinator);
        }
     
    }

}