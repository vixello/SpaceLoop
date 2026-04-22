using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Contracts;

namespace Assets.Scripts.Services.Achievements
{
    public class RewardService
    {
        public void GrantReward(Reward reward, string achievementId)
        {
            if (!reward.IsValid)
            {
                Debug.LogWarning($"No reward configured for achievement {achievementId}");
                return;
            }

            switch (reward.Type)
            {
                case RewardType.CoinTierOne:
                    //EventBus<PlayerCurrencyChanged>.Raise(new PlayerCurrencyChanged(reward.Amount)); 
                    break;
                case RewardType.Item:
                    //EventBus<ItemGranted>.Raise(new ItemGranted(reward.ItemId)); 
                    break;
                case RewardType.Xp:
                    EventBus<PlayerXpGained>.Raise(new PlayerXpGained(reward.Amount)); 
                    break;
                case RewardType.Title:
                    EventBus<TitleUnlocked>.Raise(new TitleUnlocked
                    {
                        Id = achievementId
                    });
                    break;
                default:
                    Debug.LogWarning("Unknown reward type");
                    break;
            }
        }
    }
}
