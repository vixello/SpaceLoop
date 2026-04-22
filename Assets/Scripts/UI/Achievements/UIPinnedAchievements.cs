using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.UI.Achievements
{
    internal class UIPinnedAchievements : MonoBehaviour
    {
        [SerializeField] private UIAchievementPinned[] pinned;
        private int loadedCount = 0;

        private void Start()
        {
            foreach (var p in pinned)
            {
                if (p.IsLoaded)
                {
                    HandlePinnedLoaded();
                    continue;
                }
                p.OnLoaded += HandlePinnedLoaded;
            }
        }

        private void HandlePinnedLoaded()
        {
            loadedCount++;
            if (loadedCount == pinned.Length)
            {
                EventBus<PinnedAchievementsRequested>.Raise(new PinnedAchievementsRequested());
            }
        }
    }
}
