using DragonMUD.Network;

namespace DragonMUD.Interpreters
{
    public abstract class CommandInterpreterLogic
    {
        public CommandInterpreter Interpreter { get; internal set; }

        public abstract void DisplayHelp(ConnectionCoordinator coordinator);

        public abstract void RepeatCommands(ConnectionCoordinator coordinator);

        public abstract bool IsRepeating();
    }
}