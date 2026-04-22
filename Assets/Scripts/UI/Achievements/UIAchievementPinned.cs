using UnityEngine;
using Assets.Scripts.Core;
using UnityEngine.UI;
using System;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Assets.Scripts.UI
{
    public class AchievementCommandContext
    {
        public bool IsPinned { get; }
        public bool IsAssigned { get; }
        public string AchievementId { get; }
        public SceneTransitionSO SceneTransition { get; }

        public AchievementCommandContext(
            string achievementId,
            bool isPinned,
            bool isAssigned,
            SceneTransitionSO sceneTransition)
        {
            AchievementId = achievementId;
            IsPinned = isPinned;
            IsAssigned = isAssigned;
            SceneTransition = sceneTransition;
        }
    }


    public class UIAchievementPinned : UIAchievement
    {
        private bool _isImageSet = false;

        [SerializeField] private RawImage DefaultPinnedIcon;

        [Header("Scene Transitions")]
        [SerializeField] private SceneTransitionSO _transitionToAchievementPanelScene;

        private EventBinding<PinnedAchievementsReady> _pinnedBinding;
        public event Action OnLoaded;
        public bool IsLoaded;

        protected override void Start()
        {
            base.Start();
            _isImageSet = _iconImage.texture != null;
            _pinnedBinding = new EventBinding<PinnedAchievementsReady>(OnPinnedRequested);
            EventBus<PinnedAchievementsReady>.Register(_pinnedBinding);
            gameObject.SetActive(true);

            OnLoaded?.Invoke();
            IsLoaded = true;
        }

        protected virtual void OnDestroy()
        {
            if (_pinnedBinding != null)
            {
                EventBus<PinnedAchievementsReady>.Deregister(_pinnedBinding);
                _pinnedBinding = null;
            }
        }

        public override async void OnButtonClicked()
        {
            Debug.Log($"[ACHIEV] OnPinnedClicked {Id}");

            if (!_isImageSet)
            {
                Debug.Log("[ACHIEV] Image is null, moving to achievement pining");
                var context = new AchievementCommandContext
                (
                    achievementId: null,
                    isPinned: false,
                    isAssigned: false,
                    sceneTransition: _transitionToAchievementPanelScene
                );
                var command = _commandFactory.Create();
                await command.Execute(context);
            }
            else
            {
                // if image is set, go to the achievement detail scene
                base.OnButtonClicked();
            }
        }

        private void OnPinnedRequested(PinnedAchievementsReady ready)
        {
            SetPinnedData(ready.Pinned);
        }

        protected override void OnPinnedChanged(PinnedAchievementsChanged changed)
        {
            SetPinnedData(changed.NewPinned);
        }

        private void SetPinnedData(PinnedAchievContext[] pinned)
        {
            if (Index >= pinned.Length)
            {
                SetIconTexture(null);
                return;
            }

            Id = pinned[Index].Id;
            SetIconTexture(pinned[Index].Texture);
        }

        public override void SetIconTexture(Texture2D texture)
        {
            if (!this || _iconImage == null || !_iconImage)
                return;
            if (texture == null)
            {
                _iconImage.gameObject.SetActive(false);
                DefaultPinnedIcon.gameObject.SetActive(true); 
                _isImageSet = false;
                return;
            }
            _isImageSet = true;
            _iconImage.texture = texture;
            _iconImage.gameObject.SetActive(true);
            DefaultPinnedIcon.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }
}
