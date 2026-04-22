using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using Data;
using System;

namespace Tests.Assets.Tests.Mock
{
    public class MockSaveSystem : ISaveSystem
    {
        public UniTask ClearLocalData() => UniTask.CompletedTask;

        public UniTask Load() => UniTask.CompletedTask;

        public UniTask<AutomaticLoginData> LoadLoginCache()
        {
            return new UniTask<AutomaticLoginData>();
        }

        public void Register(ISaveable saveable) { }

        public void RequestSave() { }

        public UniTask SaveLocally() => UniTask.CompletedTask;

        public UniTask SaveLoginCache(AutomaticLoginData data) => UniTask.CompletedTask;

        public void Unregister(ISaveable saveable) { }

        public UniTask WaitUntilReady() => UniTask.CompletedTask;
    }

    public class MockDialogService : IDialogService
    {
        // --- Error Dialog ---
        public string LastErrorMessage { get; private set; }

        public void ShowError(string message)
        {
            LastErrorMessage = message;
        }

        // --- Account Transfer Dialog ---
        public string LastAccountTransferMessage { get; private set; }
        public DialogChoiceCallback LastAccountTransferCallback { get; private set; }

        public void ShowAccountTransfer(string message, DialogChoiceCallback onChoice)
        {
            LastAccountTransferMessage = message;
            LastAccountTransferCallback = onChoice;
        }

        // --- Device Logout Dialog ---
        public string LastDeviceLogoutMessage { get; private set; }

        // What the mock should return when awaited
        public DialogChoice DeviceLogoutChoiceToReturn { get; set; } = DialogChoice.Cancel;

        // Optional: capture the callback if you want to simulate the dialog's internal behavior
        public Action<DialogChoice, Action, Action> DeviceLogoutCallback { get; private set; }

        public UniTask<DialogChoice> ShowDeviceLogout(string message)
        {
            LastDeviceLogoutMessage = message;

            DeviceLogoutCallback = (choice, onSuccess, onFail) =>
            {
                DeviceLogoutChoiceToReturn = choice;
                onSuccess?.Invoke();
            };

            return UniTask.FromResult(DeviceLogoutChoiceToReturn);
        }
    }

}
