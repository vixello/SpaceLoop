using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Services.Auth
{
    /// <summary>
    /// Provides account login strategy for login using Epic Online Services
    /// </summary>
    public sealed class EosAccountLoginStrategy : IAccountLoginStrategy
    {
        public UniTask Login(CancellationToken token, LoginType loginType)
        {
            Debug.Log($"[AUTH] EosAccountLoginStrategy {loginType}");
            var tcs = new UniTaskCompletionSource();
            AuthAwaiter.Login = tcs;
            
            EventBus<LoginEvent>.Raise(new LoginEvent
            {
                LoginType = loginType
            });
            token.Register(() => tcs.TrySetCanceled());
            return tcs.Task;
        }

        public UniTask Logout(CancellationToken token, LoginType loginType)
        {
            var tcs = new UniTaskCompletionSource();
            AuthAwaiter.Logout = tcs; 
            
            EventBus<LogoutEvent>.Raise(new LogoutEvent
            {
                LoginType = loginType
            });
            token.Register(() => tcs.TrySetCanceled());
            return tcs.Task;
        }
    }
}

