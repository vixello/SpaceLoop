using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IInitializable
    {
        public UniTask Initialize();
        public bool IsInitialized();
/*        public void Register(IInitializer initializer = null);
        public void Unregister(IInitializer initializer = null);*/
    }
}
