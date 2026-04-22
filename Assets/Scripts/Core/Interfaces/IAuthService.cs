using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IAuthService
    {
        public bool AppFirstLogin { get; set; }
        public bool IsPersistentLogin { get; }
        public UniTask Login(LoginType loginType);
        public UniTask Logout(LoginType loginType);
        public void SwitchAccount();
        public void LinkAccount();

    }
}
