using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Services.Auth;
using Assets.Scripts.Services.Eos;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Services.EOS.Auth
{
    public class DeviceAuthProvider : IAuthProvider, IInitializable
    {
/*        public bool IsLoggedIn => _isLoggednIn;
        private bool _isLoggednIn;

        public string DisplayName => _displayName;
        private string _displayName;*/
        public bool IsInitialized() => _isInitialized;
        private bool _isInitialized;

        [Inject] private ILoginBackendProvider _loginBackendProvider;
        DeviceIdentityService _deviceIdentityService;

        private UILoginMenuEOS _uiLoginMenuEos;

        public DeviceAuthProvider()
        {
            _deviceIdentityService = new DeviceIdentityService();
        }

        public async UniTask Initialize()
        {
            _uiLoginMenuEos = await _loginBackendProvider.GetBackend();
            _isInitialized = true;
        }

        public async UniTask<LoginResult> LoginAsync(CancellationToken token, Action<bool> action)
        {
            if (_uiLoginMenuEos == null) _uiLoginMenuEos = await _loginBackendProvider.GetBackend();

            var tcs = new UniTaskCompletionSource<LoginResult>();

            token.ThrowIfCancellationRequested();
            Debug.LogWarning("[AUTH] LoginAsync triggered");

            if (_uiLoginMenuEos == null)
            {
                Debug.LogWarning("UILoginMenu has not been set");
                tcs.TrySetResult(new LoginResult()
                {
                    EosResult = Result.Canceled,
                    UserId = "",
                    Status = AuthResult.Failed
                });
            }
            else
            {
                Debug.LogWarning($"[AUTH] _uiLoginMenuEos {_uiLoginMenuEos}");
            }

            using (var registration = token.Register(() => tcs.TrySetCanceled()))
            {
                _uiLoginMenuEos.SetAuthProviderLoginCallback(info =>
                {
                    var result = new LoginResult
                    {
                        EosResult = info.ResultCode,
                        UserId = info.LocalUserId?.ToString(),
                        Status = info.ResultCode == Result.Success
                            ? AuthResult.Success
                            : AuthResult.Failed
                    };
                    tcs.TrySetResult(result);
                });

            }

            _uiLoginMenuEos.OnLoginTypeChanged(5);
            if (_uiLoginMenuEos.connectTypeDropdown != null)
            {
                _uiLoginMenuEos.connectTypeDropdown.value = 1;
                _uiLoginMenuEos.OnConnectDropdownChange();
            }
            else
            {
                Debug.LogError("[AUTH] connectTypeDropdown is null");
            }
            _uiLoginMenuEos.OnLoginButtonClick();

            return await tcs.Task;
        }

        public async UniTask<LogoutResult> Logout(CancellationToken token)
        {
            if (_uiLoginMenuEos == null) _uiLoginMenuEos = await _loginBackendProvider.GetBackend();
            token.ThrowIfCancellationRequested();
            var tcsLogout = new UniTaskCompletionSource<LogoutResult>();

            IAccountLoginStrategy strategy = new EosAccountLoginStrategy();
            await strategy.Login(token, LoginType.DeviceId);
            await _uiLoginMenuEos.UnlinkCurrentDeviceAsync();

            _uiLoginMenuEos.SetAuthProviderLogoutCallback((info, sceneName) =>
            {
                Debug.Log("[AUTH] OnLogoutResult invoked");
                var result = new LogoutResult
                {
                    SceneName = sceneName,
                    Status = info.ResultCode == Result.Success
                        ? AuthResult.Success
                        : AuthResult.Failed
                };
                tcsLogout.TrySetResult(result);
            });

            _uiLoginMenuEos.OnLogoutButtonClick();
            return await tcsLogout.Task;
        }

        public async UniTask SwitchAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> onAccountSwithcReady)
        {
            Debug.Log("[AUTH] Switching account (TEST MODE)");

            // 1. Logout (fire-and-forget)
            EventBus<LogoutEvent>.Raise(new LogoutEvent
            {
                LoginType = LoginType.DeviceId
            });

            // 🔴 Give EOS time to logout
            await UniTask.Delay(500, cancellationToken: token);

            // 2. Delete Device ID
            await _deviceIdentityService.DeleteDeviceIdAsync();

            // 3. Create Device ID
            await _deviceIdentityService.CreateDeviceIdAsync();

            // 🔴 Give OS keychain time to settle
            await UniTask.Delay(200, cancellationToken: token);

            // 4. Login with Device ID
            EventBus<LoginEvent>.Raise(new LoginEvent
            {
                LoginType = LoginType.DeviceId
            });

            // 🔴 Wait for device login to finish
            await UniTask.Delay(1000, cancellationToken: token);

            // 5. Login with Google
            EventBus<LoginEvent>.Raise(new LoginEvent
            {
                LoginType = LoginType.GoogleId
            });

            Debug.Log("[AUTH] SwitchAccount sequence fired");
        }

        public async UniTask LinkAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> localDataWipeDecision)
        {
            if (_uiLoginMenuEos == null) _uiLoginMenuEos = await _loginBackendProvider.GetBackend();
            Debug.Log("[AUTH] Linking account");

            // No logout
            // No delete device ID
            // No create device ID
            _uiLoginMenuEos.DeviceIdProductUser = EOSManager.Instance.GetProductUserId();
            _uiLoginMenuEos.ManualLoginType = LoginType.GoogleId;
            await strategy.Login(token, LoginType.GoogleId);
            _uiLoginMenuEos.SetOnTransferDecision(localDataWipeDecision);

            // EOS will trigger your ConnectLoginTokenCallback
            // If PUID mismatch → TransferDeviceIdAccount will run
        }
    }
}
