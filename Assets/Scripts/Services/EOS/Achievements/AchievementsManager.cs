using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data.Achievements;
using Cysharp.Threading.Tasks;
using Data;
using Epic.OnlineServices.Achievements;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using Assets.Scripts.Contracts;

namespace Assets.Scripts.Services.Achievements
{
    public class AchievementsManager : MonoBehaviour, ISaveable
    {
        private List<IUIAchievement> _achievementListItems = new();

        [Header("UI Related objects")]
        [SerializeField] private GameObject _itemTemplate;
        [SerializeField] private Transform _achievementListContainer;

        [Header("Rewards for Achievements")]
        [SerializeField] private AchievementRewardDatabase _rewardDatabase;
        private RewardService _rewardService = new RewardService();

        private int _displayIndex = -1;

        private const uint _maxPinned = 4;
        private List<AchievementData> _achievementDataList = new List<AchievementData>();
        private Dictionary<string, LocalAchievementSaveData> _localAchievementSavedData = new Dictionary<string, LocalAchievementSaveData>();

        private EventBinding<AchievementUnlocked> _achievementUnlockedBinding;
        private EventBinding<AchievPinToggleRequested> _achievPinToggleRequested;
        private EventBinding<AchievementUIReady> _uiReadyBinding;
        private EventBinding<PinnedAchievementsRequested> _pinnedRequestedBinding;
        private EventBinding<AchievementDetailRequested> _achievementDetailRequestedBinding;
        private EventBinding<RewardClaimRequest> _rewardClaimRequestBinding;

        private EOSService.ServiceUpdatedEventHandler _achievementUpdatedHandler;
        private IObjectResolver _objectResolver;

        private PinnedAchievementManager _pinnedAchievementManager;

        [Inject]
        private void Construct(IObjectResolver objectResolver, Func<Dictionary<string, LocalAchievementSaveData>, uint, PinnedAchievementManager> createPinnedManager)
        {
            _objectResolver = objectResolver;
            _pinnedAchievementManager = createPinnedManager(_localAchievementSavedData, _maxPinned);
        }

        public void Start()
        {
            _achievementUnlockedBinding = new EventBinding<AchievementUnlocked>(UnlockAchievement);
            //_loginSuccededBinding = new EventBinding<LoginSucceded>(OnEosAchievementsReady);
            _achievPinToggleRequested = new EventBinding<AchievPinToggleRequested>(TogglePin);
            _uiReadyBinding = new EventBinding<AchievementUIReady>(OnUiReady);
            _pinnedRequestedBinding = new EventBinding<PinnedAchievementsRequested>(OnPinnedRequested);
            _achievementDetailRequestedBinding = new EventBinding<AchievementDetailRequested>(SendAchievmentData);
            _rewardClaimRequestBinding = new EventBinding<RewardClaimRequest>(ClaimReward);
            
            EventBus<AchievementUnlocked>.Register(_achievementUnlockedBinding);
            //EventBus<LoginSucceded>.Register(_loginSuccededBinding);
            EventBus<AchievPinToggleRequested>.Register(_achievPinToggleRequested);
            EventBus<AchievementUIReady>.Register(_uiReadyBinding);
            EventBus<PinnedAchievementsRequested>.Register(_pinnedRequestedBinding);
            EventBus<AchievementDetailRequested>.Register(_achievementDetailRequestedBinding);
            EventBus<RewardClaimRequest>.Register(_rewardClaimRequestBinding);

            _achievementUpdatedHandler = () =>
            {
                OnAchievementDataUpdated().Forget(Debug.LogException);
            };
            AchievementsService.Instance.Updated += _achievementUpdatedHandler;
            ClearAchievUi();
        }

        private void OnDestroy()
        {
            //EventBus<LoginSucceded>.Deregister(_loginSuccededBinding);
            EventBus<AchievementUnlocked>.Deregister(_achievementUnlockedBinding);
            EventBus<AchievPinToggleRequested>.Deregister(_achievPinToggleRequested);
            EventBus<AchievementUIReady>.Deregister(_uiReadyBinding);
            EventBus<PinnedAchievementsRequested>.Deregister(_pinnedRequestedBinding);
            EventBus<AchievementDetailRequested>.Deregister(_achievementDetailRequestedBinding);
            EventBus<RewardClaimRequest>.Deregister(_rewardClaimRequestBinding);
            AchievementsService.Instance.Updated -= _achievementUpdatedHandler;
        }

        public async void IncrementStat(string statName, int amount)
        {
            await StatsService.Instance.IngestStatAsync(statName, amount);
        }

        private async UniTask OnAchievementDataUpdated()
        {
            if (AchievementsService.Instance == null)
            {
                Debug.Log("[ACHIEV] AchievementsService.Instance is null");
                return;
            }

            await WaitForAchievementsCache();

            Debug.Log("[ACHIEV] OnAchievementDataUpdated called");
            ClearAchievUi();

            uint achievementDefCount;
            try
            {
                achievementDefCount = AchievementsService.GetAchievementsCount();
            }
            catch (Exception e)
            {
                Debug.LogError("[ACHIEV] GetAchievementsCount failed");
                Debug.LogException(e);
                return;
            }

            if (achievementDefCount > 0)
            {
                Utils.ExpectedAchievementsCount = achievementDefCount;

                BuildRuntimeAchievementData();
                bool localChanged = MergeEosIntoLocalSave();

                if (localChanged)
                {
                    EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested());
                }
                Debug.Log("[ACHIEV] Attempting to achievementDataListCopy");

                List<AchievementData> achievementDataListCopy = new(_achievementDataList);
                foreach (var achievementData in achievementDataListCopy)
                {
                    await AddAchievementButton(achievementData);
                }
            }
            else
            {
                Debug.LogError("[ACHIEV] No Achievements Found");
            }
            RefreshDisplayingDefinition();
        }
        private async UniTask WaitForAchievementsCache(
            CancellationToken token = default)
        {
            // Optional: add a timeout to avoid infinite waiting
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(10)); // adjust as needed

            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();

                var service = AchievementsService.Instance;

                if (service != null)
                {
                    var cache = service.CachedAchievements();

                    if (cache != null && cache.Any())
                        return;
                }

                await UniTask.Delay(100, cancellationToken: cts.Token);
            }
        }


        private void ClearAchievUi()
        {
            if (_achievementListItems != null)
            {
                foreach (var item in _achievementListItems)
                {
                    if (item == null)
                        continue;

                    item.Destroy();
                }
            }
            _achievementListItems.Clear();
            _achievementDataList.Clear();
        }

        private void BuildRuntimeAchievementData()
        {
            Debug.Log("[ACHIEV] Attempting to BuildRuntimeAchievementData");
            IEnumerable<DefinitionV2> cachedAchievs = AchievementsService.Instance.CachedAchievements();

            if(_rewardDatabase == null)
            {
                Debug.Log("[ACHIEV] _rewardDatabase is null");
            }
            foreach (var def in cachedAchievs)
            {
                Debug.Log($"[ACHIEV] _achievementDataList adding runtimeData {def.AchievementId}");
                _achievementDataList.Add(new AchievementData
                {
                    Definition = def,
                    PlayerData = null,
                    Reward = _rewardDatabase.GetReward(def.AchievementId)
                });
            }

            Debug.Log($"[ACHIEV] GetProductUserId");
            var userId = EOSManager.Instance.GetProductUserId();
            if (userId.IsValid())
            {
                foreach (var playerAch in AchievementsService.Instance.CachedPlayerAchievements(userId))
                {
                    var ach = _achievementDataList
                        .Find(a => a.Definition.AchievementId == playerAch.AchievementId);

                    if (ach != null)
                        ach.PlayerData = playerAch;
                }
            }
            else
            {
                Debug.Log($"[ACHIEV] userId not valid");
            }
        }

        private bool MergeEosIntoLocalSave()
        {
            Debug.Log("[ACHIEV] Attempting to MergeEosIntoLocalSave");

            bool changed = false;

            foreach (var achievData in _achievementDataList)
            {
                if (!_localAchievementSavedData.TryGetValue(
                        achievData.Definition.AchievementId, out var local))
                {
                    local = new LocalAchievementSaveData
                    {
                        AchievementId = achievData.Definition.AchievementId
                    };
                    _localAchievementSavedData.Add(local.AchievementId, local);
                    changed = true;
                }

                bool unlocked = IsUnlocked(achievData);

                changed |= SetIfDifferent(ref local.Progress,
                    achievData.PlayerData?.Progress ?? 0);

                changed |= SetIfDifferent(ref local.IsUnlocked, unlocked);
                changed |= SetIfDifferent(ref local.UnlockTime,
                    achievData.PlayerData?.UnlockTime);

                if (achievData.PlayerData.HasValue && achievData.PlayerData.Value.StatInfo != null)
                {
                    if (local.StatProgress == null)
                    {
                        local.StatProgress = new Dictionary<string, double>();
                        changed = true;
                    }

                    foreach (var s in achievData.PlayerData.Value.StatInfo)
                    {
                        if (!local.StatProgress.TryGetValue(s.Name, out var v) || v != s.CurrentValue)
                        {
                            local.StatProgress[s.Name] = s.CurrentValue;
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
                Debug.Log("[ACHIEV] Local achievement save updated from EOS");

            return changed;
        }

        private static bool SetIfDifferent<T>(ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                return true;
            }
            return false;
        }

        public void RefreshDisplayingDefinition()
        {
            if (_displayIndex == -1)
            {
                return;
            }
            OnDefinitionIdButtonClicked(_displayIndex);
        }

        private async UniTask AddAchievementButton(AchievementData achievement)
        {
            Debug.Log("[ACHIEV] AddAchievementButton called");
            if (_achievementListContainer == null)
            {
                Debug.LogWarning("[ACHIEV] Skipping AddAchievementButton: container is null");
                return;
            }

            bool hasPlayerData = achievement.PlayerData.HasValue;
            string achievementId = achievement.Definition.AchievementId;

            if(_rewardDatabase == null) {  Debug.Log("[ACHIEV] _rewardDatabase is null"); }

            Reward reward = _rewardDatabase.GetReward(achievementId);
            IUIAchievement template = CreateTemplateInstance();

            if (template == null) { return; }

            var ctx = await BuildUIContext(achievement);
            template.Apply(ctx);
            _achievementListItems.Add(template);
        }

        private async UniTask<UIAchievementContext> BuildUIContext(AchievementData data)
        {

            var save = _localAchievementSavedData[data.Definition.AchievementId];
            bool unlocked = IsUnlocked(data);

            return new UIAchievementContext
            {
                Id = data.Definition.AchievementId,
                Title = data.Definition.UnlockedDisplayName,
                IsUnlocked = unlocked,
                IsRewardClaimed = save.IsRewardClaimed,

                Progress = GetAchievementStatInfo(data, data.PlayerData.HasValue).CurrentValue,
                MaxProgress = GetMaxProgress(data),

                IsPinned = save.IsPinned,
                PinOrder = save.PinOrder,

                Reward = _rewardDatabase.GetReward(data.Definition.AchievementId),
                Icon = await GetAchievementIcon(data.Definition.AchievementId, unlocked),
            };
            /*            template.IsUnlocked = hasPlayerData && data.PlayerData.Value.UnlockTime != null;
                        template.IsRewardClaimed = save.IsRewardClaimed;
                        template.Index = _achievementListItems.Count;
                        template.SetId(achievementId);
                        template.SetNameText(achievementId);
                        template.SetReward(reward);

                        bool unlocked = IsUnlocked(data);
                        template.SetBackgroundColor(unlocked);

                        Texture2D tex = await GetAchievementIcon(achievementId, unlocked);
                        template.SetIconTexture(tex);
                        return;*/
        }

        private static bool IsUnlocked(AchievementData achievement)
        {
            return achievement.PlayerData.HasValue && achievement.PlayerData.Value.Progress >= 1
                && achievement.PlayerData.Value.UnlockTime != null;
        }

        private LocalAchievementSaveData GetOrCreateLocalSaveData(AchievementData achievement)
        {
            if (!_localAchievementSavedData.TryGetValue(achievement.Definition.AchievementId, out var localData))
            {
                localData = new LocalAchievementSaveData
                {
                    AchievementId = achievement.Definition.AchievementId
                };
                _localAchievementSavedData.Add(localData.AchievementId, localData);
            }

            return localData;
        }

        private IUIAchievement CreateTemplateInstance()
        {
            if(_itemTemplate == null)
            {
                Debug.Log("[ACHIEV] _itemTemplate is null"); 
                return null;    
            }
            GameObject button = Instantiate(_itemTemplate, _achievementListContainer);

            IUIAchievement template = button.GetComponent<IUIAchievement>();
            _objectResolver.Inject(template);

            button.SetActive(true); 
            return template;
        }

        private static PlayerStatInfo GetAchievementStatInfo(AchievementData achievement, bool hasPlayerData)
        {
            PlayerStatInfo statInfo;
            if (hasPlayerData && achievement.PlayerData.Value.StatInfo != null && achievement.PlayerData.Value.StatInfo.Length > 0)
            {
                statInfo = achievement.PlayerData.Value.StatInfo[0];
            }
            else
            {
                statInfo = new PlayerStatInfo
                {
                    Name = "none",
                    CurrentValue = 0,
                    ThresholdValue = 0
                };
            }

            return statInfo;
        }

        private static async UniTask<Texture2D> GetAchievementIcon(string achievementId, bool unlocked)
        {
            Task<Texture2D> getIconTextureTask = unlocked
                ? AchievementsService.Instance.GetAchievementUnlockedIconTexture(achievementId)
                : AchievementsService.Instance.GetAchievementLockedIconTexture(achievementId);

            var tex = await getIconTextureTask;
            return tex;
        }

        private void ClaimReward(RewardClaimRequest e)
        {
            var reward = _rewardDatabase.GetReward(e.AchievementId);
            var save = _localAchievementSavedData[e.AchievementId];

            if (save == null)
            {
                Debug.LogWarning($"No save data found for achievement {e.AchievementId}");
                return;
            }
            if (IsUnlocked(_achievementDataList.Find(a => a.PlayerData.Value.AchievementId == e.AchievementId)) && !save.IsRewardClaimed)
            {
                _rewardService.GrantReward(reward, e.AchievementId);
                save.IsRewardClaimed = true;
                EventBus<RewardClaimed>.Raise(new RewardClaimed
                {
                    AchievementId = e.AchievementId,
                });                 
                EventBus<AchievementStateChanged>.Raise(new AchievementStateChanged { }); 

            }
        }

        private async void RaisePinnedChangedEvent()
        {
            PinnedAchievContext[] newPinned = await GetPinnedAchievContexts();

            EventBus<PinnedAchievementsChanged>.Raise(new PinnedAchievementsChanged
            {
                NewPinned = newPinned
            });

            // save
            EventBus<AchievementStateChanged>.Raise(new AchievementStateChanged { });
        }

        private void OnPinnedRequested(PinnedAchievementsRequested evt) { HandlePinnedRequested().Forget(); }
        private async UniTask HandlePinnedRequested()
        {
            PinnedAchievContext[] newPinned;
            PinnedAchievCache[] cache = _pinnedAchievementManager?.GetPinnedCache();
            Debug.Log("[ACHIEV] GetPinnedCache ");

            if (cache == null)
            {
                Debug.Log("[ACHIEV] GetPinnedCache null");
                newPinned = await GetPinnedAchievContexts();
            }
            else
            {
                Debug.Log("[ACHIEV] GetPinnedCache BuildPinnedContextsAsync");
                newPinned = await BuildPinnedContextsAsync(cache.Select(c => (c.AchievementId, c.PinOrder)).ToList());
            }

            EventBus<PinnedAchievementsReady>.Raise(new PinnedAchievementsReady
            {
                Pinned = newPinned
            });
        }

        private async void SendAchievmentData(AchievementDetailRequested requested)
        {
            Debug.Log("[ACHIEV] SendAchievmentData ");
            if (_achievementDataList == null || _achievementDataList.Count() == 0)
            {
                Debug.Log("[ACHIEV] _achievementDataList null"); 
                await OnAchievementDataUpdated();
            }
            if (requested.AchievementId == null) Debug.Log("[ACHIEV] requested.AchievementId is null"); 

            var data = _achievementDataList.Find(a => a.Definition.AchievementId == requested.AchievementId);
            bool hasPlayerData = data.PlayerData.HasValue;
            if (data != null)
            {
                Debug.Log("[ACHIEV] SENDING DATA to detail ");
                PlayerStatInfo statInfo = GetAchievementStatInfo(data, hasPlayerData);
                AchievementDetailContext context = new AchievementDetailContext
                {
                    IsUnlocked = IsUnlocked(data),
                    IsRewardClaimed = _localAchievementSavedData[requested.AchievementId].IsRewardClaimed,
                    UnlockDate = data.PlayerData.Value.UnlockTime,
                    Title = data.Definition.UnlockedDisplayName,
                    Progress = statInfo.CurrentValue,
                    MaxProgress = GetMaxProgress(data),
                    Detail = data.Definition.UnlockedDescription,
                    Texture = await GetAchievementIcon(data.Definition.AchievementId, true)
                };
                EventBus<AchievementDetailReady>.Raise(new AchievementDetailReady 
                {
                    AchievementDetail = context
                });
            }
            else{
                Debug.Log("data is null");
            }
        }

        private async UniTask<PinnedAchievContext[]> GetPinnedAchievContexts()
        {
            List<LocalAchievementSaveData> pinned = GetPinnedAchievements();
            var source = pinned.Select(p => (p.AchievementId, p.PinOrder)).ToList();

            PinnedAchievContext[] newPinned = await BuildPinnedContextsAsync(source);

            _pinnedAchievementManager.SetPinnedCache(newPinned);
            return newPinned;
        }

        private async UniTask<PinnedAchievContext[]> BuildPinnedContextsAsync(IReadOnlyList<(string AchievementId, int PinOrder)> source)
        {
            var result = new PinnedAchievContext[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                result[i] = new PinnedAchievContext
                {
                    Id = source[i].AchievementId,
                    PinOrder = source[i].PinOrder,
                    Texture = await GetAchievementIcon(source[i].AchievementId, true)
                };
            }

            return result;
        }

        private List<LocalAchievementSaveData> GetPinnedAchievements()
        {
            return _localAchievementSavedData.Values
                .Where(a => a.IsPinned)
                .OrderBy(a => a.PinOrder)
                .ToList();
        }

        private async void OnUiReady(AchievementUIReady ready)
        {
            Debug.Log("[ACHIEV] OnUiReady called");
            if(ready.Container == null)
            {
                Debug.Log("[ACHIEV] Container is null");
                return;
            }
            await AchievementsService.Instance.RefreshAsync();
            _achievementListContainer = ready.Container;
            await OnAchievementDataUpdated();
        }

        public void OnDefinitionIdButtonClicked(int i)
        {
            if (i > AchievementsService.GetAchievementsCount())
            {
                return;
            }

            _displayIndex = i;

            var achievementData = _achievementDataList[i];
            var definition = achievementData.Definition;

            // Set both icons to be hidden
            //achievementLockedIcon.gameObject.SetActive(false);
            //achievementUnlockedIcon.gameObject.SetActive(false);

            // Asynchronously retrieve the icons and set the textures
            // DisplayPlayerAchievement then will set the appropriate icon to be visible
            //achievementUnlockedIcon.texture = await AchievementsService.Instance.GetAchievementUnlockedIconTexture(definition.AchievementId);
            //achievementLockedIcon.texture = await AchievementsService.Instance.GetAchievementLockedIconTexture(definition.AchievementId);

            //unlockAchievementButton.gameObject.SetActive(true);

            //if (displayDefinition)
            //{
            //    DisplayAchievementDefinition(definition);
            //}
            //else
            //{
            //    DisplayPlayerAchievement(definition);
            //}

            //unlockAchievementButton.interactable = achievementData.PlayerData.HasValue && achievementData.PlayerData?.Progress < 1;

        }

        //TODO: refresh data data without having to log out
        /// <summary>
        /// Manually unlock achievement being displayed
        /// </summary>
        /// <param name="unlockedAchievement"></param>
        private async void UnlockAchievement(AchievementUnlocked unlockedAchievement)
        {
            try
            {
                await AchievementsService.Instance.UnlockAchievementAsync(unlockedAchievement.AchievementId);
                EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested { });
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

/*        private void OnEosAchievementsReady()
        {
            ClearAchievUi();
            BuildRuntimeAchievementData();
            MergeEosIntoLocalSave();
            EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested());
        }
*/
        private void TogglePin(AchievPinToggleRequested e)
        {
            if (e.AchievementId == null) return;

            bool changed = _pinnedAchievementManager.TogglePin(e.AchievementId);    
            if (changed)
            {
                _pinnedAchievementManager.ClearPinnedCache();
                RaisePinnedChangedEvent();
            }
        }

        public void SaveData(ref SaveContainer data)
        {
            if (data.Achievements == null)
                data.Achievements = new List<LocalAchievementSaveData>();

            data.Achievements.Clear();

            foreach (var kvp in _localAchievementSavedData)
            {
                if (kvp.Value == null)
                {
                    Debug.LogWarning($"[ACHIEV] Null achievement entry for key {kvp.Key}");
                    continue;
                }

                data.Achievements.Add(kvp.Value);
            }

            Debug.Log($"[ACHIEV] Saved {data.Achievements.Count} achievements");
        }

        public async UniTask LoadData(SaveContainer data)
        {
            _localAchievementSavedData.Clear();

            // If no save exists, create entries instead of returning
            if (data?.Achievements == null || data.Achievements.Count == 0)
            {
                Debug.Log("[ACHIEV] No local achievement save found — creating entries");

                foreach (var def in AchievementsService.Instance.CachedAchievements())
                {
                    _localAchievementSavedData[def.AchievementId] = new LocalAchievementSaveData
                    {
                        AchievementId = def.AchievementId,
                        Progress = 0,
                        IsUnlocked = false,
                        IsRewardClaimed = false,
                    };
                }

                return;
            }

            // Load saved achievements
            foreach (var achiev in data.Achievements) 
            {
                _localAchievementSavedData[achiev.AchievementId] = achiev; 
            }

            Debug.Log($"[ACHIEV] Loaded {_localAchievementSavedData.Count} achievements");
            await GetPinnedAchievContexts();

            await UniTask.CompletedTask;
        }

        public void ClearData()
        {
            _localAchievementSavedData = new Dictionary<string, LocalAchievementSaveData>();
        }

        private int GetMaxProgress(AchievementData data)
        {
            if (data.Definition.StatThresholds != null &&
                data.Definition.StatThresholds.Length > 0)
            {
                return data.Definition.StatThresholds[0].Threshold;
            }

            return 0;
        }
    }
}
