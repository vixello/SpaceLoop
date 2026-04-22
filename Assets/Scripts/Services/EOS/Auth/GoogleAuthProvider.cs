using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Services.Auth;
using Assets.Scripts.Services.Eos;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Internal;

namespace Assets.Scripts.Services.EOS.Auth
{
    public interface IDeviceIdentityService
    {
        bool HasDeviceId { get; }
        ProductUserId CurrentDevicePuid { get; }

        UniTask<Result> CreateDeviceIdAsync();
        UniTask<Result> DeleteDeviceIdAsync();
    }

    public class GoogleAuthProvider : IAuthProvider, IInitializable
    {
/*        public bool IsLoggedIn => _isLoggednIn;
        private bool _isLoggednIn;

        public string DisplayName => _displayName;
        private string _displayName;*/
        public bool IsInitialized() => _isInitialized;
        private bool _isInitialized = false;

        [Inject] private ILoginBackendProvider _loginBackendProvider;
        [Inject] private IDialogService _dialogService;
        [Inject] private IInitializer _initializer;
        DeviceIdentityService _deviceIdentityService;

        private UILoginMenuEOS _uiLoginMenuEos;


        public GoogleAuthProvider() 
        {
            _deviceIdentityService = new DeviceIdentityService();
        }

        public async UniTask Initialize()
        {
            _uiLoginMenuEos = await _loginBackendProvider.GetBackend();
/*                 
            // Uncomment if there is a need to get user permission to transfer   
            _uiLoginMenuEos.SetAccountTransferCallback(ShowAccountTransferDialog);
*/
            _deviceIdentityService = new DeviceIdentityService();

            Debug.Log("[AUTH] GoogleAuthProvider initialized");
            _isInitialized = true;
        }

        public async UniTask<LoginResult> LoginAsync(CancellationToken token, Action<bool> action) 
        {
            if(_uiLoginMenuEos == null) _uiLoginMenuEos = await _loginBackendProvider.GetBackend();

            var tcs = new UniTaskCompletionSource<LoginResult>();
            _uiLoginMenuEos.SetOnTransferDecision(action);

            token.ThrowIfCancellationRequested();
            Debug.Log("[AUTH] Google auth: LoginAsync triggered");

            Debug.Log("[AUTH] Google auth: attempting deviceId reset");
            //Result deleteResult = await _deviceIdentityService.DeleteDeviceIdAsync();
            //Result createResult = await _deviceIdentityService.CreateDeviceIdAsync();

            Debug.Log("[AUTH] Google auth: creating a login strategy for device");
            IAccountLoginStrategy strategy = new EosAccountLoginStrategy();
            await strategy.Login(token, LoginType.DeviceId);

            if (_uiLoginMenuEos == null)
            {
                Debug.Log("[AUTH] Google auth: UILoginMenu has not been set");
                tcs.TrySetResult(new LoginResult()
                {
                    EosResult = Result.Canceled,
                    UserId = "",
                    Status = AuthResult.Failed
                });
            }
            else
            {
                Debug.LogWarning($"[AUTH] Google auth: _uiLoginMenuEos {_uiLoginMenuEos}");
            }

            using (var registration = token.Register(() => tcs.TrySetCanceled()))
            {
                _uiLoginMenuEos.SetAuthProviderLoginCallback(info =>
                {
                    Debug.Log($"[AUTH] AuthProviderLoginCallback triggered");
                    var result = new LoginResult
                    {
                        EosResult = info.ResultCode,
                        UserId = info.LocalUserId?.ToString(),
                        Status = info.ResultCode == Result.Success
                                                    ? AuthResult.Success
                                                    : info.ResultCode == Result.Canceled
                                                        ? AuthResult.Canceled
                                                        : AuthResult.Failed
                    };
                    tcs.TrySetResult(result);
                });

            }

            _uiLoginMenuEos.OnLoginTypeChanged(5);
            if (_uiLoginMenuEos.connectTypeDropdown != null)
            {
                _uiLoginMenuEos.connectTypeDropdown.value = 3;
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
            if(_uiLoginMenuEos == null) _uiLoginMenuEos = await _loginBackendProvider.GetBackend();
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

        /// <summary>
        /// Shows the account‑transfer dialog when the backend requests it,
        /// forwarding the caller's choice callback to the dialog.
        /// </summary>
        /// <param name="onChoiceCallback"></param>
        private void ShowAccountTransferDialog(DialogChoiceCallback onChoiceCallback)
        {
            _dialogService.ShowAccountTransfer(null, onChoiceCallback);
        }

        public async UniTask SwitchAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> onAccountSwitchReady)
        {
            Debug.Log("[AUTHSWITCH] Switching account (TEST MODE)");

            await strategy.Logout(token, LoginType.DeviceId);

            Result deleteResult = await _deviceIdentityService.DeleteDeviceIdAsync();
            Result createResult = await _deviceIdentityService.CreateDeviceIdAsync();

            bool canWipeIdentity =
                deleteResult == Result.Success &&
                createResult == Result.Success;

            onAccountSwitchReady?.Invoke(!canWipeIdentity);

            await strategy.Login(token, LoginType.DeviceId);
            await strategy.Login(token, LoginType.GoogleId);

            Debug.Log("[AUTHSWITCH] SwitchAccount sequence completed");
        }

        public UniTask LinkAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback)
        {
            throw new NotImplementedException();
        }
    }

    public class GuestAuthProvider : IAuthProvider
    {
        public async UniTask<LoginResult> LoginAsync(CancellationToken token, Action<bool> action)
        {
            await UniTask.Yield();
            return new LoginResult();
            /* EOS logic */
        }
        public UniTask SwitchAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback)
        {
            throw new NotImplementedException();
        }
        public async UniTask<LogoutResult> Logout(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var tcs = new UniTaskCompletionSource<LogoutResult>();

            return await tcs.Task;
        }

        public UniTask LinkAccount(CancellationToken token, IAccountLoginStrategy strategy, Action<bool> resultCallback)
        {
            throw new NotImplementedException();
        }

/*        public bool IsLoggedIn => false *//* check local flag *//*;
        public string DisplayName => "Guest";*/
    }

}
