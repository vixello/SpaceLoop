using Assets.Scripts.Core;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public abstract class DialogBase : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _message;

        public DialogChoiceCallback OnChoiceMade;

        public virtual void Show(string message)
        {
            if(message != null)
            {
                _message.text = message;
            }
            gameObject.SetActive(true);
        }

        public virtual void Hide(bool toDestroy = true)
        {
            gameObject.SetActive(false);
            if(toDestroy) Destroy(gameObject);
        }

        public void AcceptClicked()
        {
            ChoiceClicked(DialogChoice.Accept);
        }

        public void CancelClicked()
        {
            ChoiceClicked(DialogChoice.Cancel);
        }

        /// <summary>
        /// Triggers the choice callback and supplies an onComplete action.
        /// The dialog will hide only when the caller invokes onComplete().
        /// </summary>
        /// <param name="dialogChoice"></param>
        public virtual void ChoiceClicked(DialogChoice dialogChoice, Action onSuccessCallback = null, Action<string> onFailCallback = null)
        {
            onSuccessCallback ??= () => Hide(true); 
            onFailCallback ??= (err) => Debug.LogError(err);

            OnChoiceMade?.Invoke(dialogChoice, onSuccessCallback, onFailCallback);
        }
    }
}

