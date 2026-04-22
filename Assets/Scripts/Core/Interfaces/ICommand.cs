using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface ICommand<TContext>
    {
        UniTask Execute(TContext context);
    }
}
