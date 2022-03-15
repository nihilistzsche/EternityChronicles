namespace DragonMUD.StateMachine.Helpers
{
    public interface IMenu
    {
        string MenuLine { get; }

        string KeyForInfo { get; }
    }
}