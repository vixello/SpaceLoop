using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Achievements
{
    public class UIAchievementDetail : MonoBehaviour, IInitializable
    {
        [Header("Basic Achievement Data")]
        [SerializeField] private RawImage _achievementIcon;
        [SerializeField] private TMP_Text _achievemntTitle;
        [SerializeField] private TMP_Text _achievementDetailTextContainer;
        [SerializeField] private TMP_Text _unlockDate;

        [Header("Progress Data")]
        [SerializeField] private Slider _achievementProgressSlider;
        [SerializeField] private GameObject _achievementProgressSliderGo;
        [SerializeField] private TMP_Text _achievementProgressText;

        EventBinding<AchievementDetailReady> _achievementReadyBinding;

        public UniTask Initialize()
        {
            _achievementReadyBinding = new EventBinding<AchievementDetailReady>(FillInAcheviementDetail);
            EventBus<AchievementDetailReady>.Register(_achievementReadyBinding);
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            EventBus<AchievementDetailReady>.Deregister(_achievementReadyBinding);
        }

        private void FillInAcheviementDetail(AchievementDetailReady ready)
        {
            var achievData = ready.AchievementDetail;

            _achievementIcon.texture = achievData.Texture;
            _achievemntTitle.text = achievData.Title;
            _achievementDetailTextContainer.text = achievData.Detail;
            _unlockDate.text = achievData.IsUnlocked ?  $"Unlocked {achievData.UnlockDate}" : "LOCKED" ;
            SetProgress(achievData.Progress, achievData.Progress);
        }

        public void SetProgress(int progress, int maxProgress)
        {
            if (maxProgress == 0)
            {
                _achievementProgressSliderGo.SetActive(false);
                _achievementProgressSliderGo.SetActive(false);
                return;
            }

            _achievementProgressSlider.maxValue = maxProgress;
            _achievementProgressSlider.value = progress;
            _achievementProgressText.text = progress.ToString() + "/" + maxProgress;
        }

        public bool IsInitialized()
        {
            throw new System.NotImplementedException();
        }
    }
}
