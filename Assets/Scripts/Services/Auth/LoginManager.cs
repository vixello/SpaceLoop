using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using Data;
using System;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Services.Auth
{
    public static class AuthAwaiter
    {
        public static UniTaskCompletionSource Login;
        public static UniTaskCompletionSource Logout;
        public static void ResetLogin()
        {
            Login = new UniTaskCompletionSource();
        }

        public static void ResetLogout()
        {
            Logout = new UniTaskCompletionSource();
        }
    }


    public class LoginManager : MonoBehaviour
    {
        [SerializeField] private SceneTransitionSO _sceneTransition;

        private EventBinding<LoginEvent> _loginEventBinding;
        private EventBinding<LogoutEvent> _logoutEventBinding;
        private EventBinding<SwitchAccountEvent> _switchAccountEventBinding;
        private EventBinding<LinkAccountEvent> _linkAccountEventBinding;
        private EventBinding<SceneLoadedEvent> _sceneLoadedEvent;
        private ISaveSystem _saveSystem;    

        /*        private IDialogService _dialogService;
                private IAuthProviderFactory _authProviderFactory;
                private IIdentityManager _identityManager;
                private ISaveSystem _saveSystem;
                private Func<ILoadingScope> _beginLoading;*/

        private IAuthService _authService;

        [Inject]
        private void Construct(IDialogService dialogSerivce,
                                IAuthProviderFactory authProviderFactory,
                                IIdentityManager identityManager,
                                ISaveSystem saveSystem,
                                Func<ILoadingScope> func)
        {
            _saveSystem = saveSystem;
            _authService = new AuthService(dialogSerivce, authProviderFactory, identityManager, saveSystem, func, _sceneTransition);
            _authService.AppFirstLogin = false;
/*            _dialogService = dialogSerivce;
            _authProviderFactory = authProviderFactory;
            _identityManager = identityManager;
            _saveSystem = saveSystem;   
            _beginLoading = func;*/
        }

        private void Awake()
        {
            _loginEventBinding = new EventBinding<LoginEvent>(OnLoginEvent);
            _logoutEventBinding = new EventBinding<LogoutEvent>(OnLogoutEvent);
            _switchAccountEventBinding = new EventBinding<SwitchAccountEvent>(OnSwitchAccountEvent);
            _linkAccountEventBinding = new EventBinding<LinkAccountEvent>(OnLinkAccountEvent);
            _sceneLoadedEvent = new EventBinding<SceneLoadedEvent>(OnSceneLoadedEvent);
            EventBus<LoginEvent>.Register(_loginEventBinding);
            EventBus<LogoutEvent>.Register(_logoutEventBinding);
            EventBus<SwitchAccountEvent>.Register(_switchAccountEventBinding);
            EventBus<LinkAccountEvent>.Register(_linkAccountEventBinding);
            EventBus<SceneLoadedEvent>.Register(_sceneLoadedEvent);
        }

        private void OnDisable()
        {
            EventBus<LoginEvent>.Deregister(_loginEventBinding);
            EventBus<LogoutEvent>.Deregister(_logoutEventBinding);
            EventBus<SwitchAccountEvent>.Deregister(_switchAccountEventBinding);
            EventBus<LinkAccountEvent>.Deregister(_linkAccountEventBinding);
            EventBus<SceneLoadedEvent>.Deregister(_sceneLoadedEvent);
        }

        private async void OnLoginEvent(LoginEvent e)
        {
            await _authService.Login(e.LoginType);
        }

        private async void OnLogoutEvent(LogoutEvent e)
        {
            await _authService.Logout(e.LoginType);
        }

        private void OnSwitchAccountEvent(SwitchAccountEvent e)
        {
            _authService.SwitchAccount();
        }

        private void OnLinkAccountEvent(LinkAccountEvent e)
        {
            _authService.LinkAccount();
        }

        private void OnSceneLoadedEvent(SceneLoadedEvent e)
        {
            if (!_authService.AppFirstLogin && _authService.IsPersistentLogin && e.SceneName == SceneNames.LoginScene)
            {
                EventBus<LoginEvent>.Raise(new LoginEvent { LoginType = LoginType.DeviceId });
            }
        }

/*        private async UniTask Login(LoginType loginType)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                string message = ErrorMessages.NoInternet;
                _dialogService.ShowError(message);
                return;
            }

            IAuthProvider authProvider = await GetAuthProvider(loginType);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy()
            );

            try
            {
                using (var loading = _beginLoading())
                {
                    loading.SetMessage(StringConstants.LoggingIn);

                    var result = await authProvider.LoginAsync(cts.Token);
                    HandleLoginResult(result, loginType);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[AUTH] Login task canceled");
            }
        }*/
/*
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
        }*/

/*        private async UniTask Logout(LoginType loginType)
        {
            IAuthProvider authProvider = await GetAuthProvider(loginType);
            bool isGuest = _identityManager.GetIdentity().IsGuest;
            Debug.Log($"[AUTH] Logout...IsGuest {isGuest}");
            await UniTask.SwitchToMainThread();

            if(isGuest)
            {
                var choice = await _dialogService.ShowDeviceLogout(null); 
                if (choice == DialogChoice.Cancel) 
                { 
                    Debug.Log("[AUTH] User canceled logout"); 
                    return; 
                }
                Debug.Log("[AUTH] User confirmed data erase");
            }
            using (var cts = new CancellationTokenSource())
            {
                var result = await authProvider.Logout(cts.Token);
                if(result != null) Debug.Log("[AUTH] Got result...");
                if(result == null) Debug.Log("[AUTH] Result is null...");
                HandleLogoutResult(result);
            }
        }*/

        /// <summary>
        /// Use EosGoogleAccountSwitchStrategy to provide 
        /// </summary>
/*        public async void SwitchAccount()
        {
            // 1. Save current identity if needed
            _saveSystem.RequestSave();
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

                //_saveSystem.RequestSave();
            }
            else
            {
                return;
            }

            Debug.Log($"[AUTH] Performed switch");
        }*/

/*        public async void LinkAccount()
        {
            // 1. Save current identity if needed
            _saveSystem.RequestSave();
            EventBus<CloudSaveDataRequested>.Raise(new CloudSaveDataRequested
            {
                FileName = StringConstants.SaveFileName,
                Provider = CloudProviderType.Eos,
                Reason = CloudSyncReason.AccountSwitch
            });

            IAccountLoginStrategy strategy = new EosAccountLoginStrategy();
            IAuthProvider authProvider = await GetAuthProvider(LoginType.DeviceId);

            if (_identityManager.GetIdentity().IsGuest)
            {
                using (var cts = new CancellationTokenSource())
                {
                    await authProvider.LinkAccount(cts.Token, strategy, DecideAboutAccountWipe);
                }
                _identityManager.GetIdentity().IsGuest = false;
                _identityManager.GetIdentity().AccountTransfered = true;
                //_saveSystem.RequestSave();
            }
            else
            {
                return;
            }

            Debug.Log($"[AUTH] Performed Link");
        }

        private async void DecideAboutAccountWipe(bool canSwitch)
        {
            if(canSwitch)
            {
                _identityManager.ClearIdentity();
                await _saveSystem.ClearLocalData();
                //_saveSystem.RequestSave();
            }
        }*/
/*
        private async void HandleLoginResult(LoginResult result, LoginType loginType)
        {
            Debug.Log($"[AUTH] HandleLoginResult");
            Debug.Log($"[AUTH] Login result: {result}");
            switch (result.Status)
            {
                case AuthResult.Success:
                    AppFirstLogin = true; Debug.Log($"[AUTH] Login for {loginType} successful: {result.UserId}");

                    EventBus<ChangeButtonVisibilityEvent>.Raise(new ChangeButtonVisibilityEvent
                    {
                        ButtonType = ButtonType.Login,
                        LoginType = loginType,
                        IsActive = false
                    });

                    AuthAwaiter.Login?.TrySetResult();

                    bool isGuest = loginType == LoginType.DeviceId && !_identityManager.GetIdentity().AccountTransfered;
                    if (loginType != LoginType.DeviceId) { isGuest = false; }

                    EventBus<LoginSucceded>.Raise(new LoginSucceded
                    {
                        SyncReason = Core.Interfaces.CloudSyncReason.Login,
                        CloudProviderType = Core.Interfaces.CloudProviderType.Eos,
                        IsGuest = isGuest,
                    });

                    await _saveSystem.WaitUntilReady();
                    // await _saveSystem.Load();
                    // REMEMBER TO FIX WTIH PERSISTENT LOGIN
                    ChangeSceneToMenu();
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
                    AuthAwaiter.Login?.TrySetCanceled();
                    AuthAwaiter.Login = null;
                    Debug.LogError(result.ErrorMessage +" " + result.EosResult);
                    break;
                case AuthResult.Canceled:
                    AuthAwaiter.Login?.TrySetCanceled();
                    AuthAwaiter.Login = null;
                    break;
            }
        }*/

/*        private void ChangeSceneToMenu()
        {
            EventBus<SceneTransitionEvent>.Raise(new SceneTransitionEvent
            { //change scene afterwards
                ScenesToUnload = _sceneTransition.ScenesToUnload,
                ScenesToLoad = _sceneTransition.ScenesToLoad
            });
        }*/

/*        private async void HandleLogoutResult(LogoutResult result)
        {
            Debug.Log("[AUTH] Attempting to handle logout result..."); 
            switch (result.Status)
            {
                case AuthResult.Success :
                    Debug.Log($"[AUTH] Logout succesful");
                    AuthAwaiter.Logout?.TrySetResult();
                    Debug.Log($"[AUTH] OnLogoutResult unload {result.SceneName}");
                    EventBus<LogoutSucceeded>.Raise(new LogoutSucceeded());
                    await _saveSystem.ClearLocalData();

                    break;
                case AuthResult.Failed:
                    AuthAwaiter.Login = null;
                    Debug.LogError(result.EosResult);
                    break;
            }
        }*/
    }
}

