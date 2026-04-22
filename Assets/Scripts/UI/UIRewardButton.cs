using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    internal class UIRewardButton : Button, IUIRewardButton
    {
        public void RequestRewardClaim(string id)
        {
            EventBus<RewardClaimRequest>.Raise(new RewardClaimRequest
            {
                AchievementId = id,
            });
        }
    }
}
