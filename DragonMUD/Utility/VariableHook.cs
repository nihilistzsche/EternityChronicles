using DragonMUD.Network;
using XDL;

namespace DragonMUD.Utility
{
	public class VariableHook : BaseObject, IWriteHook
	{
		public string ProcessMessage(string message)
		{
			return message.ReplaceAllVariables();
		}
	}
}
