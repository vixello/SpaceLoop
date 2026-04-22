
using UnityEngine;

namespace Assets.Scripts.Core
{
    public struct AchievementUnlocked : IEvent
    {
        public string AchievementId;
    }

    public struct AchievPinToggleRequested : IEvent
    {
        public string AchievementId;
    }

    public struct AchievementUIReady : IEvent
    {
        public Transform Container;
    }

    public struct EnableAchievementPinningMode : IEvent { }

    public struct PinnedAchievementsChanged : IEvent
    {
        public PinnedAchievContext[] NewPinned;
    }

    public struct PinnedAchievementsRequested : IEvent { }
    public struct PinnedAchievementsReady : IEvent
    {
        public PinnedAchievContext[] Pinned;
    }

    public struct AchievementStateChanged : IEvent { }

    public struct AchievementDetailRequested : IEvent
    {
        public string AchievementId;
    }

    public struct AchievementDetailReady : IEvent
    {
        public AchievementDetailContext AchievementDetail;
    }

}
