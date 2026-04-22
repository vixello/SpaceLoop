using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Services.Achievements
{
    public class PinnedAchievCache
    {
        public string AchievementId;
        public int PinOrder;
    }

    public class PinnedAchievementManager
    {
        private readonly Dictionary<string, LocalAchievementSaveData> _localAchievementSavedData;
        private readonly uint _maxPinned = 4;
        private PinnedAchievCache[] _cachedPinned;

        public PinnedAchievementManager(Dictionary<string, LocalAchievementSaveData> localAchievementSavedData, uint maxPinned)
        {
            _localAchievementSavedData = localAchievementSavedData;
            _maxPinned = maxPinned;
        }

        // becasue each acheivement is toggled invidually,
        // we have to somehow save hte data only if the player hits save fir the pinned achievements/
        // or should we save wit heevery toggle?
        public bool TogglePin(string achievementId)
        {
            if (!_localAchievementSavedData.TryGetValue(achievementId, out var achiev)) return false;

            // UNPIN → stop here
            if (achiev.IsPinned)
            {
                Unpin(achiev);
                return true;
            }

            if (GetPinnedCount() >= _maxPinned)
                return false;

            Pin(achiev);
            return true;
        }

        private void Pin(LocalAchievementSaveData achiev)
        {
            achiev.IsPinned = true;
            achiev.PinOrder = GetNextPinOrder();
        }

        private void Unpin(LocalAchievementSaveData achiev)
        {
            achiev.IsPinned = false;
            achiev.PinOrder = -1;
            RecalculatePinOrders();
        }

        public int GetPinnedCount() => _localAchievementSavedData.Values.Count(a => a.IsPinned);

        public void SetPinnedCache(PinnedAchievContext[] pinned)
        {
            if (pinned == null || pinned.Length == 0)
            {
                Debug.Log("SetPinnedCache is null");
                _cachedPinned = Array.Empty<PinnedAchievCache>();
                return;
            }

            _cachedPinned = new PinnedAchievCache[pinned.Length];

            for (int i = 0; i < pinned.Length; i++)
            {
                Debug.Log($"Set Pinned {pinned[i].Id}");
                _cachedPinned[i] = new PinnedAchievCache
                {
                    AchievementId = pinned[i].Id,
                    PinOrder = pinned[i].PinOrder
                };
            }
        }


        public void ClearPinnedCache() { _cachedPinned = null; }

        public PinnedAchievCache[] GetPinnedCache()
        {
            if (_cachedPinned.Length == 0) {
                Debug.Log($"GetPinnedCache no caches for pinned"); 
            }
            return _cachedPinned;
        }

        private int GetNextPinOrder()
        {
            return _localAchievementSavedData.Values
                .Where(a => a.IsPinned)
                .Select(a => a.PinOrder)
                .Max() + 1;
        }

        private void RecalculatePinOrders()
        {
            int order = 0;

            IOrderedEnumerable<LocalAchievementSaveData> pinned = _localAchievementSavedData.Values
                .Where(a => a.IsPinned)
                .OrderBy(a => a.PinOrder);

            foreach (var pin in pinned)
            {
                pin.PinOrder = order;
                order++;
            }
        }
    }
}
