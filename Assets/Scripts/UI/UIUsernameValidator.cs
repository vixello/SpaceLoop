using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Services.Auth;
using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.UI
{

    /// <summary>
    /// Checks username corectness and availability
    /// </summary>
    public class UIUsernameValidator : MonoBehaviour, IDisposable, IInitializable
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Button _submitButton;

        private IUsernameValidationSystem _validation; 
        private IIdentityManager _identity;
        private ISaveSystem _saveSystem;

        [Inject] 
        private void Construct(IUsernameValidationSystem validation, IIdentityManager identity, ISaveSystem saveSystem) 
        { 
            _validation = validation; 
            _identity = identity;
            _saveSystem = saveSystem;
        }

        public async UniTask Initialize()
        {
            await _saveSystem.WaitUntilReady();

            Debug.Log($"Username validator");
            while (_identity == null)
            {
                await UniTask.Yield();
            }

            Debug.Log($"Username identity not null");
            gameObject.SetActive(true);
            if (_identity.IsUsernameSet())
            {
                Debug.Log($"Username arleady set");
                Dispose();
                return;
            }
            _submitButton.onClick.AddListener(OnSubmit);
            _errorText.text = "";
        }

        private void OnSubmit()
        {
            var result = _validation.Validate(_usernameInput.text.Trim());

            if (result.ErrorType != UsernameErrorType.Allowed)
            {
                ShowError(result.Message);
                return;
            }

            // Username OK
            _errorText.text = "";
            ProceedWithUsername(_usernameInput.text.Trim());
            Dispose();
        }

        private void ShowError(string message)
        {
            _errorText.text = message;
        }

        private void ProceedWithUsername(string username)
        {
            Debug.Log($"Username accepted: {username}");
            EventBus<UsernameAccepted>.Raise(new UsernameAccepted
            {
                Username = username
            });
            gameObject.SetActive(false);
        }

        public void Dispose()
        {
            gameObject.SetActive(false);
            Debug.Log($"UIUsernameValidator Dispose");
            Destroy(this);
        }

        public bool IsInitialized()
        {
            throw new NotImplementedException();
        }
    }
}
