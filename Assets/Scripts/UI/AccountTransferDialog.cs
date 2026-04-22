using Assets.Scripts.Core;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.UI
{
    public class AccountTransferDialog : DialogBase // 3 choices here, first account, second account or cancel
    {
        [SerializeField] private SceneTransitionSO _sceneTransition;
        [SerializeField] private ErrorDialog _errorDialog;

        public void UseFirstAccountClicked()
        {
            ChoiceClicked(DialogChoice.UseDeviceAccount, () => Hide(true), (message) => ShowTransferError(message));
        }

        public void UseSecondAccountClicked()
        {
            ChoiceClicked(DialogChoice.UseGoogleAccount, () => Hide(true), (message) => ShowTransferError(message));
        }

        private void ShowTransferError(string message)
        {
            var go = Instantiate(_errorDialog, gameObject.transform);
            ErrorDialog dialog = go.GetComponent<ErrorDialog>();
            dialog.Show(ErrorMessages.TransferFailed + "\n" + message);
        }

        public override void Hide(bool toDestroy = false)
        {
            base.Hide(toDestroy);

/*            EventBus<SceneTransitionEvent>.Raise(new SceneTransitionEvent
            { //change scene afterwards
                ScenesToUnload = _sceneTransition.ScenesToUnload,
                ScenesToLoad = _sceneTransition.ScenesToLoad
            });*/
        }
    }
}

