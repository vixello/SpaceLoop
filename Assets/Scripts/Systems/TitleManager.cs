using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data.Titles;
using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Systems
{
    public class TitleManager : MonoBehaviour, ITitleManager
    {
        private TitleDatabase _titleDatabase;
        private List<TitleSaveData> _titleSaveData = new();
        private List<RuntimeTitle> _runtimeTitles = new();

        public IReadOnlyList<RuntimeTitle> Titles => _runtimeTitles;
        private EventBinding<AchievementUnlocked> _achievementUnlockedBinding;
        private EventBinding<TitleClicked> _titleClickedBinding;
        private EventBinding<TitleUnlockRequested> _titleUnlockRequestedBinding;

        [Inject]
        private void Construct(TitleDatabase titleDatabase)
        {
            _titleDatabase = titleDatabase;
        }

        public UniTask Initialize()
        {
            BuildRuntimeTitles();
            _achievementUnlockedBinding = new EventBinding<AchievementUnlocked>(CheckForTitleUnlock);
            _titleClickedBinding = new EventBinding<TitleClicked>(SelectTitle);
            _titleUnlockRequestedBinding = new EventBinding<TitleUnlockRequested>(UnlockTitle);
            EventBus<AchievementUnlocked>.Register(_achievementUnlockedBinding);
            EventBus<TitleClicked>.Register(_titleClickedBinding);
            EventBus<TitleUnlockRequested>.Register(_titleUnlockRequestedBinding);

            return UniTask.CompletedTask;   
        }

        private void OnDestroy()
        {
            EventBus<AchievementUnlocked>.Deregister(_achievementUnlockedBinding);
            EventBus<TitleClicked>.Deregister(_titleClickedBinding);
            EventBus<TitleUnlockRequested>.Deregister(_titleUnlockRequestedBinding);
        }

        private void CheckForTitleUnlock(AchievementUnlocked e)
        {
            foreach (var rt in _runtimeTitles)
            {
                if (rt.Config.RequiredAchievementId == e.AchievementId &&
                    !rt.Save.IsUnlocked)
                {
                    UnlockTitle(new TitleUnlockRequested { Id = rt.Config.TitleId });
                }
            }
        }

        private void BuildRuntimeTitles()
        {
            _runtimeTitles.Clear();

            foreach (var config in _titleDatabase.Titles)
            {
                var save = _titleSaveData.Find(t => t.TitleId == config.TitleId);

                if (save == null)
                {
                    save = new TitleSaveData
                    {
                        TitleId = config.TitleId,
                        IsUnlocked = false,
                        IsSelected = false,
                        lastUpdatedUtc = 0
                    };
                    _titleSaveData.Add(save);
                }

                _runtimeTitles.Add(new RuntimeTitle
                {
                    Config = config,
                    Save = save
                });
            }
        }

        public void UnlockTitle(TitleUnlockRequested e)
        {
            var rt = _runtimeTitles.Find(t => t.Config.TitleId == e.Id);
            if (rt == null) return;

            rt.Save.IsUnlocked = true;
            rt.Save.lastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            EventBus<TitleUnlocked>.Raise(new TitleUnlocked
            {
                Id = e.Id,
            });
            EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested { });
        }

        public void SelectTitle(TitleClicked e)
        {
            Debug.Log($"[TITLE] select {e.Id}");
            foreach (var rt in _runtimeTitles)
                rt.Save.IsSelected = false;

            var selected = _runtimeTitles.Find(t => t.Config.TitleId == e.Id);
            if (selected != null)
            {
                selected.Save.IsSelected = true;
                selected.Save.lastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            EventBus<TitleSelected>.Raise(new TitleSelected
            {
                Id = e.Id,
            });
            EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested { });
        }

        public void SaveData(ref SaveContainer data)
        {
            if(_titleSaveData == null) return;
            if (data.TitleSaveData == null)
            {
                data.TitleSaveData = new List<TitleSaveData>();
            }

            data.TitleSaveData = _titleSaveData;

            var selected = data.TitleSaveData.Find(t => t.IsSelected);
            if (selected == null)
            {
                Debug.LogWarning("No selected title found. Using default title.");
/*                var title = data.TitleSaveData.Find(t => t.TitleId == "NEWCOMER");
                title.IsSelected = true;
                title.IsUnlocked = true;
                selected = title;*/
                return;
            }

            if(_runtimeTitles ==  null)
            {
                Debug.LogError($"Runtime title config not found");
                return;
            }
            Debug.Log($"selected {selected}");
            Debug.Log($"configForSelected {_runtimeTitles.Find(t => t.Config.TitleId == selected?.TitleId).Config}");

            var configForSelected = _runtimeTitles.Find(t => t.Config.TitleId == selected?.TitleId).Config;
            data.Identity.CurrentTitleId = configForSelected?.TitleId;
            Debug.Log($"SaveData title {data.Identity.CurrentTitleId}");
        }

        public UniTask LoadData(SaveContainer data)
        {
            if (data == null || data.TitleSaveData == null || data.TitleSaveData.Count == 0)
            {
                Debug.Log("No local titleData save found — keeping runtime data");
                return UniTask.CompletedTask;
            }
            Debug.Log($"LoadData {data.TitleSaveData}");

            _titleSaveData = data.TitleSaveData ?? new List<TitleSaveData>();
            ReevaluateAllTitles(data.Achievements);
            BuildRuntimeTitles();

            return UniTask.CompletedTask;
        }

        public void ClearData()
        {
            _titleSaveData = new List<TitleSaveData>();
            _runtimeTitles = new List<RuntimeTitle>();
        }

        private void ReevaluateAllTitles(List<LocalAchievementSaveData> data)
        {
            var unlocked = new List<string>();
            foreach (var achievement in data)
            {
                if(achievement != null && achievement.IsUnlocked)
                {
                    unlocked.Add(achievement.AchievementId);
                }
            }

            foreach (var rt in _titleSaveData)
            {
                if (rt.TitleId == "NEWCOMER") rt.IsUnlocked = true;
                if (!rt.IsUnlocked &&
                    data.Any(a => a.AchievementId == rt.RequiredAchievementId))
                {
                    UnlockTitle(new TitleUnlockRequested { Id = rt.TitleId });
                }
            }
        }

        public bool IsInitialized()
        {
            throw new NotImplementedException();
        }
    }
}
