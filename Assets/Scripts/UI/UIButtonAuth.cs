using Assets.Scripts.Core;
using System;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class UIButtonAuth : MonoBehaviour
    {
        [SerializeField] LoginType _loginType;
        [SerializeField] ButtonType _buttonType;
        private EventBinding<ChangeButtonVisibilityEvent> _buttonVisibilityBinding;

        private void Start()
        {
            _buttonVisibilityBinding = new EventBinding<ChangeButtonVisibilityEvent>(OnVisibikityCHanged);
            EventBus<ChangeButtonVisibilityEvent>.Register(_buttonVisibilityBinding);
        }

        private void OnDestroy()
        {
            EventBus<ChangeButtonVisibilityEvent>.Deregister(_buttonVisibilityBinding);
        }

        private void OnVisibikityCHanged(ChangeButtonVisibilityEvent visibility)
        {
            if (_buttonVisibilityBinding != null && visibility.ButtonType == _buttonType 
                && visibility.LoginType == _loginType)
            {
                gameObject.SetActive(visibility.IsActive);
            }
        }

        public void StartLogin()
        {
            EventBus<LoginEvent>.Raise(new LoginEvent
                {
                    LoginType = _loginType
                }   
            );
        }
        public void StartLoginSequence()
        {
            EventBus<LoginEvent>.Raise(new LoginEvent
            {
                LoginType = _loginType
            }
            );
        }
        public void SwitchAccount()
        {
            EventBus<SwitchAccountEvent>.Raise(new SwitchAccountEvent { });
        }

        public void LinkAccount()
        {
            EventBus<LinkAccountEvent>.Raise(new LinkAccountEvent { });
        }

        public void StartLogout()
        {
            EventBus<LogoutEvent>.Raise(new LogoutEvent
                {
                    LoginType = _loginType
                }
            );
        }
    }
}
