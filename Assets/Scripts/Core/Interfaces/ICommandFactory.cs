namespace Assets.Scripts.Core.Interfaces
{
    public interface ICommandFactory<TCommand>
    {
        TCommand Create();
    }

}
