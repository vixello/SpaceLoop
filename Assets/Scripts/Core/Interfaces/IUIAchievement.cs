using UnityEngine;
using Assets.Scripts.Contracts;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IUIAchievement
    {
        public bool IsUnlocked { get; set; }
        public bool IsRewardClaimed { get; set; }
        public int Index { get;  set; }
        public string Id { get;}

        public void Apply(UIAchievementContext ctx);

        public void SetButtonListener(string id);
        void SetIconTexture(Texture2D texture);
        void SetNameText(string Text);
        void OnButtonClicked();
        void Destroy();
        void SetProgress(int progress, int maxProgress);
        void SetBackgroundColor(bool isUnlocked);
        void SetReward(Reward reward);
        void SetPin(bool isPinned, int index);
        void SetPinMode(bool pinModeEnabled);
    }
}
