using DragonMUD.Interpreters;
using DragonMUD.Network;

namespace DragonMUD.StateMachine
{
    public interface IState
    {
        void Process(ConnectionCoordinator coordinator, string input);

        void SendSoftRebootMessage(ConnectionCoordinator coordinator);
        
		BaseInterpreter Interpreter { get; }
    }
}