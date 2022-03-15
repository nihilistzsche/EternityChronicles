using DragonMUD.Interpreters;
using DragonMUD.Network;

namespace DragonMUD.StateMachine
{
    public interface IState
    {
        BaseInterpreter Interpreter { get; }
        void            Process(ConnectionCoordinator coordinator, string input);

        void SendSoftRebootMessage(ConnectionCoordinator coordinator);
    }
}