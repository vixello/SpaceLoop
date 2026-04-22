using System;
using System.Threading;
using Assets.Scripts.Core;
using Assets.Scripts.Services.Auth;
using Cysharp.Threading.Tasks;

namespace Tests.Assets.Tests.Mock
{
    public class MockAuthProviderFactory : IAuthProviderFactory
    {
        private readonly IAuthProvider _provider;

        public MockAuthProviderFactory(IAuthProvider provider)
        {
            _provider = provider;
        }

        public IAuthProvider GetProvider(LoginType type)
        {
            return _provider;
        }
    }

    public class MockAuthProvider : IAuthProvider
    {
        private readonly LoginResult _mockLoginResult;
        private readonly LogoutResult _mockLogoutResult;

        public MockAuthProvider(LoginResult loginResult)
        {
            _mockLoginResult = loginResult;
        }
        public MockAuthProvider(LoginResult loginResult,
                                LogoutResult logoutResult)
        {
            _mockLoginResult = loginResult;
            _mockLogoutResult = logoutResult;
        }

        public UniTask<LoginResult> LoginAsync(CancellationToken token, Action<bool> action)
        {
            return UniTask.FromResult(_mockLoginResult);
        }

        public UniTask<LogoutResult> Logout(CancellationToken token)
        {
            return UniTask.FromResult(_mockLogoutResult);
        }

        public UniTask SwitchAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback)
        {
            return UniTask.CompletedTask;
        }
        public UniTask LinkAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback)
        {
            return UniTask.CompletedTask;
        }
    }
}
