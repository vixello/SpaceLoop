using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.PlayerIdentity
{
    public class UIPictureTemplate : MonoBehaviour, IUIPictureTemplate
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _imageFrame;
        [SerializeField] TMP_Text _levelToUnlock;
        private string _id;
        public string Id => _id;
        private bool _isUnlocked;

        public void SetId(string id)
        {
            _id = id;
        }

        public void SetIamgeData(Sprite sprite, int levelToUnlock)
        {
            if (sprite == null) return;
            _image.sprite = sprite;
            _levelToUnlock.text = "Level" + levelToUnlock.ToString();
        }

        public void SetUnlockStatus(bool isUnlocked)
        {
            _isUnlocked = isUnlocked;
            _image.color = isUnlocked ? Color.white : Color.gray;
        }
        public void SetSelectedStatus(bool isSelected)
        {
            _imageFrame.gameObject.SetActive(isSelected);
        }

        public void OnImageClicked()
        {
            if(!_isUnlocked) return;
            EventBus<PFPClicked>.Raise(new PFPClicked
            {
                Id = _id,
            });
        }
    }
}
