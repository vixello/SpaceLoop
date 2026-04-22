using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.PlayerIdentity
{
    public class UITitleTemplate : MonoBehaviour, IUITitleTemplate
    {
        [SerializeField] private TMP_Text _displayName;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Image _bgImage;
        [SerializeField] private Color _bgLockedColor;
        [SerializeField] private Color _bgUnlockedColor;
        [SerializeField] private Color _selectedColor;

        private EventBinding<TitleSelected> _titleSelectedBdining;

        private string _id;
        public string Id => _id;
        public bool _isSelected;
        public bool _isUnlocked;

        public event Action<string> OnClicked;

        private void Start()
        {
            _titleSelectedBdining = new EventBinding<TitleSelected>(OnTitleSelected);
            EventBus<TitleSelected>.Register(_titleSelectedBdining);
        }

        private void OnDestroy()
        {
            EventBus<TitleSelected>.Deregister(_titleSelectedBdining);
        }

        private void OnTitleSelected(TitleSelected selected)
        {
            if(selected.Id == _id)
            {
                SetSelectedState(true);
            }
        }

        public void SetId(string id)
        {
            _id = id ?? "None";
        }

        public void SetDisplayName(string DisplayName)
        {
            _displayName.text = DisplayName ?? "None";
        }

        public void SetUnlockState(bool isUnlocked)
        {
            _isUnlocked = isUnlocked;
            _bgImage.color = isUnlocked ? _bgUnlockedColor : _bgLockedColor;
        }

        public void SetSelectedState(bool isSelected)
        {
            _isSelected = isSelected;
            _bgImage.color = isSelected ? _selectedColor : _bgImage.color;
        }

        public void SetDescription(string description)
        {
            _description.text = description ?? "None";  
        }

        public void OnClickButton() { OnClicked?.Invoke(_id); }

        public void SelectTitle()
        {
            if (_isUnlocked)
            {
                EventBus<TitleClicked>.Raise(new TitleClicked
                {
                    Id = _id,
                });
            }
        }
    }
}