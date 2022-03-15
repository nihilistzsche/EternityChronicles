namespace DragonMUD.StateMachine.Helpers
{
    public interface IAccountMenu : IMenu
    {
        // List<Requirement> Requirements { get; }

        int Priority { get; }
    }
}