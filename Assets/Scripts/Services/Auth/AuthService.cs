using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Services.Auth
{
    public class AuthService : IAuthService
    {
        public bool AppFirstLogin { get;  set; }
        public bool IsPersistentLogin { get;  set; }
        private AutomaticLoginData _automaticLoginData;

        private IDialogService _dialogService;
        private IAuthProviderFactory _authProviderFactory;
        private IIdentityManager _identityManager;
        private ISaveSystem _saveSystem;
        private Func<ILoadingScope> _beginLoading;
        private SceneTransitionSO _sceneTransition;

        public AuthService(
                IDialogService dialogService, 
                IAuthProviderFactory authProviderFactory,
                IIdentityManager identityManager,
                ISaveSystem saveSystem,
                Func<ILoadingScope> beginLoading,
                SceneTransitionSO sceneTransition)
        {
            _dialogService = dialogService;
            _authProviderFactory = authProviderFactory;
            _identityManager = identityManager;
            _saveSystem = saveSystem;
            _beginLoading = beginLoading;
            _sceneTransition = sceneTransition;
            _ = GetPersistentLoginFlag();
        }

        public async UniTask Login(LoginType loginType)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                string message = ErrorMessages.NoInternet;
                _dialogService.ShowError(message);
                return;
            }

            IAuthProvider authProvider = await GetAuthProvider(loginType);

            /*            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                            this.GetCancellationTokenOnDestroy()
                        );*/
            var cts = new CancellationTokenSource();

            try
            {
                using (var loading = _beginLoading())
                {
                    loading.SetMessage(StringConstants.LoggingIn);

                    var result = await authProvider.LoginAsync(cts.Token, DecideAboutAccountWipe);
                    await HandleLoginResult(result, loginType);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[AUTH] Login task canceled");
            }
        }

        private async void SetPersistentLoginFlag(bool isPersistentLogin)
        {
            await _saveSystem.SaveLoginCache(new AutomaticLoginData { IsPersistentLogin = isPersistentLogin});
        }

        private async UniTask<bool> GetPersistentLoginFlag()
        {
            _automaticLoginData = await _saveSystem.LoadLoginCache();
            IsPersistentLogin = _automaticLoginData.IsPersistentLogin;
            return IsPersistentLogin;
        }

        private async UniTask HandleLoginResult(LoginResult result, LoginType loginType)
        {
            Debug.Log($"[AUTH] HandleLoginResult");
            Debug.Log($"[AUTH] Login result: {result}");
            switch (result.Status)
            {
                case AuthResult.Success:
                    AppFirstLogin = true; Debug.Log($"[AUTH] Login for {loginType} successful: {result.UserId}");
                    bool isDeviceLogin = loginType == LoginType.DeviceId;
                    await GetPersistentLoginFlag();

                    EventBus<ChangeButtonVisibilityEvent>.Raise(new ChangeButtonVisibilityEvent
                    {
                        ButtonType = ButtonType.Login,
                        LoginType = loginType,
                        IsActive = false
                    });

                    AuthAwaiter.Login?.TrySetResult();

                    bool isGuest = loginType == LoginType.DeviceId && !_identityManager.GetIdentity().AccountTransfered;
                    if (!isDeviceLogin) 
                    { 
                        isGuest = false;
                        SetPersistentLoginFlag(true);
                    }
                    if (!isDeviceLogin || (IsPersistentLogin && isDeviceLogin))
                    {
                        EventBus<LoginSucceded>.Raise(new LoginSucceded
                        {
                            SyncReason = Core.Interfaces.CloudSyncReason.Login,
                            CloudProviderType = Core.Interfaces.CloudProviderType.Eos,
                            IsGuest = isGuest,
                        });
                        await _saveSystem.WaitUntilReady();
                        ChangeSceneToMenu();
                    }
#if UNITY_EDITOR
                    EventBus<LoginSucceded>.Raise(new LoginSucceded
                    {
                        SyncReason = Core.Interfaces.CloudSyncReason.Login,
                        CloudProviderType = Core.Interfaces.CloudProviderType.Eos,
                        IsGuest = isGuest,
                    });
                    await _saveSystem.WaitUntilReady();
                    ChangeSceneToMenu();
#endif

                    // await _saveSystem.Load();
                    // REMEMBER TO FIX WTIH PERSISTENT LOGIN
                    AuthAwaiter.ResetLogin();
                    break;

                case AuthResult.NeedsAccountCreation:
                    Debug.Log($"User must create a new account for {loginType}.");
                    break;

                case AuthResult.NeedsLinking:
                    Debug.Log($"User must link an account for {loginType}.");
                    break;

                case AuthResult.NeedsTranfsering:
                    Debug.Log($"User should transfer the account for {loginType}.");
                    break;

                case AuthResult.Failed:
                    Debug.LogError($"{result.ErrorMessage ?? "Unknown error"} {result.EosResult}");
                    CancelPendingLogin();
                    break;
                case AuthResult.Canceled:
                    Debug.Log($"[AUTH] HandleLoginResult AuthResult.Canceled");
                    CancelPendingLogin();
                    break;
            }
        }

        private void CancelPendingLogin()
        {
            if (AuthAwaiter.Login != null)
            {
                AuthAwaiter.Login.TrySetCanceled();
                AuthAwaiter.Login = null;
            }
#if !UNITY_EDITOR
            _dialogService.ShowError(ErrorMessages.LoginFailed);
#endif
            AuthAwaiter.ResetLogin();
        }

        public async UniTask Logout(LoginType loginType)
        {
            IAuthProvider authProvider = await GetAuthProvider(loginType);
            bool isGuest = _identityManager.GetIdentity().IsGuest;

            Debug.Log($"[AUTH] Logout...");
            await UniTask.SwitchToMainThread();

/*            if (isGuest)
            {
                var choice = await _dialogService.ShowDeviceLogout(null);
                if (choice == DialogChoice.Cancel)
                {
                    Debug.Log("[AUTH] User canceled logout");
                    return;
                }
                Debug.Log("[AUTH] User confirmed data erase");
            }*/
            using (var cts = new CancellationTokenSource())
            {
                var result = await authProvider.Logout(cts.Token);
                if (result != null) Debug.Log("[AUTH] Got result...");
                if (result == null) Debug.Log("[AUTH] Result is null...");
                await HandleLogoutResult(result);
            }
        }
        private async UniTask HandleLogoutResult(LogoutResult result)
        {
            Debug.Log("[AUTH] Attempting to handle logout result...");
            switch (result.Status)
            {
                case AuthResult.Success:
                    Debug.Log($"[AUTH] Logout succesful");
                    AuthAwaiter.Logout?.TrySetResult();
                    Debug.Log($"[AUTH] OnLogoutResult unload {result.SceneName}");

                    SetPersistentLoginFlag(false);

                    EventBus<LogoutSucceeded>.Raise(new LogoutSucceeded());
                    await _saveSystem.ClearLocalData();
                    AuthAwaiter.ResetLogout();

                    break;
                case AuthResult.Failed:
                    AuthAwaiter.Login = null;
                    Debug.LogError(result.EosResult);
                    break;
            }
        }

        public async void SwitchAccount()
        {
            Debug.Log($"[AUTH] SwitchAccount");
            // 1. Save current identity if needed
            await _saveSystem.SaveLocally();
            EventBus<CloudSaveDataRequested>.Raise(new CloudSaveDataRequested
            {
                FileName = StringConstants.SaveFileName,
                Provider = CloudProviderType.Eos,
                Reason = CloudSyncReason.AccountSwitch
            });

            IAccountLoginStrategy strategy = new EosAccountLoginStrategy();
            IAuthProvider authProvider = await GetAuthProvider(LoginType.GoogleId);

            if (!_identityManager.GetIdentity().IsGuest)
            {
                using (var cts = new CancellationTokenSource())
                {
                    await authProvider.SwitchAccount(cts.Token, strategy, DecideAboutAccountWipe);
                }
                _identityManager.GetIdentity().IsGuest = false;
                _identityManager.GetIdentity().AccountTransfered = true;

                _saveSystem.RequestSave();
            }
            else
            {
                return;
            }

            Debug.Log($"[AUTH] Performed switch");
        }

        public async void LinkAccount()
        {
            Debug.Log($"[AUTH] LinkAccount");
            // 1. Save current identity if needed
            await _saveSystem.SaveLocally();
            EventBus<CloudSaveDataRequested>.Raise(new CloudSaveDataRequested
            {
                FileName = StringConstants.SaveFileName,
                Provider = CloudProviderType.Eos,
                Reason = CloudSyncReason.AccountSwitch
            });
            Debug.Log($"[AUTH] LinkAccount raised event");

            await _saveSystem.WaitUntilReady();

            IAccountLoginStrategy strategy = new EosAccountLoginStrategy();
            IAuthProvider authProvider = await GetAuthProvider(LoginType.DeviceId);
            Debug.Log($"[AUTH] _identityManager.GetIdentity().IsGuest {_identityManager.GetIdentity().IsGuest}");

            if (_identityManager.GetIdentity().IsGuest)
            {
                Debug.Log($"[AUTH] LinkAccount attempt, is a guest");
                using (var cts = new CancellationTokenSource())
                {
                    await authProvider.LinkAccount(cts.Token, strategy, DecideAboutAccountWipe);
                }
                _identityManager.GetIdentity().IsGuest = false;
                _identityManager.GetIdentity().AccountTransfered = true;
                _saveSystem.RequestSave();
            }
            else
            {
                return;
            }

            Debug.Log($"[AUTH] Performed Link");
        }

        private async void DecideAboutAccountWipe(bool keepDeviceData)
        {
            if (!keepDeviceData)
            {
                _identityManager.ClearIdentity();
                await _saveSystem.ClearLocalData();
                //_saveSystem.RequestSave();
            }
        }
        private async Task<IAuthProvider> GetAuthProvider(LoginType loginType)
        {
            if (_authProviderFactory == null) Debug.LogError("[AUTH] No auth provider factory assigned");

            IAuthProvider authProvider = _authProviderFactory.GetProvider(loginType);
            if (authProvider == null)
            {
                Debug.LogError("[AUTH] No auth provider assigned");
                await UniTask.Yield();
            }

            return authProvider;
        }
        private void ChangeSceneToMenu()
        {
            EventBus<SceneTransitionEvent>.Raise(new SceneTransitionEvent
            { //change scene afterwards
                ScenesToUnload = _sceneTransition.ScenesToUnload,
                ScenesToLoad = _sceneTransition.ScenesToLoad
            });
        }
    }
}

