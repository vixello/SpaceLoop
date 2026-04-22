using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.UI
{
    public class DialogService : IDialogService
    {
        [Inject] private DialogFactory _factory;
        public DialogService() {}

        public async void ShowError(string message)
        {
            var dialog = await _factory.Get<ErrorDialog>();
            dialog.Show(message);
        }

        /// <summary>
        /// Displays the account‑transfer dialog and wires the caller's callback 
        /// to the dialog's choice buttons.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="onChoice"></param>
        public async void ShowAccountTransfer(string message, DialogChoiceCallback onChoice)
        {
            var dialog = await _factory.Get<AccountTransferDialog>();
            dialog.OnChoiceMade = onChoice; 
            dialog.Show(message);
        }

        public async UniTask<DialogChoice> ShowDeviceLogout(string message)
        {
            Debug.Log("[AUTH] ShowDeviceLogout");
            var tcs = new UniTaskCompletionSource<DialogChoice>();
            var dialog = await _factory.Get<DeviceLogoutDialog>();

            dialog.Show(message);

            dialog.OnChoiceMade = (choice, onSuccess, onFail) =>
            {
                tcs.TrySetResult(choice);
                onSuccess?.Invoke();
            };

            return await tcs.Task;
        }

    }
}

