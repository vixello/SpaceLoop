using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using System;
using System.Threading;

namespace Assets.Scripts.Services.Auth
{
    public enum AuthResult
    {
        Success,
        Failed,
        NeedsAccountCreation,
        NeedsLinking,
        NeedsTranfsering,
        Canceled,
    }

    public class LoginResult
    {
        public AuthResult Status { get; set; }

        public string UserId { get; set; }         // EOS ProductUserId
        public string Token { get; set; }         // EOS ProductUserId
        public string DisplayName { get; set; }    // Google/Discord/etc
        public string ErrorMessage { get; set; }   // user-friendly text
        public Result EosResult { get; set; }      // raw EOS error

        public bool AccountCreated { get; set; }
        public bool AccountLinked { get; set; }
        public bool AccountTransferred { get; set; }
    }
    public class LogoutResult
    {
        public AuthResult Status { get; set; }
        public Result EosResult { get; set; }      // raw EOS error
        public string SceneName { get; set; }
    }

    public interface IAuthProvider
    {
        UniTask<LoginResult> LoginAsync(CancellationToken token, Action<bool> decideAboutDataCallback= null);
        UniTask<LogoutResult> Logout(CancellationToken token);
        UniTask SwitchAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback);
        UniTask LinkAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback);
/*        bool IsLoggedIn { get; }   
        string DisplayName { get; }*/
    }

    public interface IAuthProviderFactory
    {
        IAuthProvider GetProvider(LoginType type);
    }
}
