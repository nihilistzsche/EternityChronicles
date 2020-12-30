using System;
using DragonMUD.Interpreters;
using DragonMUD.Network;

namespace DragonMUD.StateMachine
{
	public class AccountNameState : BaseObject, IState
	{

		public BaseInterpreter Interpreter => null;


		public void Process(ConnectionCoordinator coordinator, string input)
		{
			var name = input;

			throw new NotImplementedException();
		}

		public void SendSoftRebootMessage(ConnectionCoordinator coordinator)
		{
			throw new NotImplementedException();
		}
	}
}
