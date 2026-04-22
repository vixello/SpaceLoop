using Cysharp.Threading.Tasks;
using System;

namespace Assets.Scripts.Core
{
    public delegate void DialogChoiceCallback(DialogChoice dialogChoice, Action onSuccess = null, Action<string> onFail = null);
    public enum DialogChoice
    {
        Accept,
        Cancel,
        UseDeviceAccount,
        UseGoogleAccount
    }

    public interface IDialogService
    {
        /// <summary>
        /// Opens the error dialog.
        /// </summary>
        /// <param name="onChoiceCallback"></param>
        void ShowError(string error);
        /// <summary>
        /// Opens the account‑transfer dialog and forwards the caller’s 
        /// choice callback to the dialog.
        /// </summary>
        /// <param name="onChoiceCallback"></param>
        public void ShowAccountTransfer(string message, DialogChoiceCallback onChoice);
        UniTask<DialogChoice> ShowDeviceLogout(string message);  
    }
}

