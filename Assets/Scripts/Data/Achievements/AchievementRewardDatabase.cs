using Assets.Scripts.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Achievements
{
    [CreateAssetMenu(menuName = "Game/Achievement Reward Database")]
    public class AchievementRewardDatabase : ScriptableObject
    {
        public List<AchievementRewardConfig> Rewards;
        public Reward GetReward(string achievementId)
        {
            foreach(var r in Rewards)
            {
                if(r.AchievementId == achievementId)
                {
                    return r.Reward;
                }
            }
            return new Reward();
        }
    }
}
