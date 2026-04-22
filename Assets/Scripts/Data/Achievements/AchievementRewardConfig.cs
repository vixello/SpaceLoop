using UnityEngine;
using Assets.Scripts.Contracts;

namespace Assets.Scripts.Data.Achievements
{
    //make a file that stores the rewards for each achievement??

    [CreateAssetMenu(menuName = "Game/Achievement Reward Config")]
    public class AchievementRewardConfig : ScriptableObject
    {
        public string AchievementId;
        public Reward Reward;
    }
}
