using Assets.Scripts.Contracts;
using Epic.OnlineServices.Achievements;
using System;

namespace Assets.Scripts.Services.Achievements
{
    [Serializable]
    class AchievementData
    {
        public DefinitionV2 Definition;
        public PlayerAchievement? PlayerData;
        public Reward Reward; // from your config
    }
}
