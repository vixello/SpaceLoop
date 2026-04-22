using Cysharp.Threading.Tasks;
using System.Threading;

namespace Assets.Scripts.Core
{
    public interface IAccountLoginStrategy
    {
        UniTask Logout(CancellationToken token, LoginType loginType);
        UniTask Login(CancellationToken token, LoginType loginType);
    }
}