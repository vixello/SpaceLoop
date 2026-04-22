using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Data;
using System;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using Assets.Scripts.Data.Titles;
using VContainer;
using Assets.Scripts.Data;


/*public interface ISaveableBase
{
    string Key { get; }
    Type DataType { get; }
    object CreateDefault();
    void SaveToContainer(ref SaveContainer container);
    void LoadFromContainer(SaveContainer container);
}*/


namespace Assets.Scripts.Systems
{

    public class SaveSystem : ISaveSystem
    {
        private static SaveContainer _localContainer = new SaveContainer();
        private static SaveContainer _cloudContainer = new SaveContainer();
        private static AutomaticLoginData _automaticLoginData = new AutomaticLoginData();

        private static List<ISaveable> _saveables = new List<ISaveable>();
        private EventBinding<CloudSaveDownloaded> _cloudSaveDwonloadedBinding;
        private EventBinding<CloudSaveDataRequested> _cloudSaveDataRequestedBinding;
        private EventBinding<LocalSaveRequested> _localSaveRequestedBinding;
        private EventBinding<UserDataRequested> _localDataRequestedBinding;
        private EventBinding<AchievementStateChanged> _achievemntStateChanged;
        private bool _isReady = false;

        private CancellationTokenSource _localSaveCts;
        private CancellationTokenSource _cloudUploadCts;
        private bool _cloudUploadInProgress = false;

        private TitleDatabase _titleDatabase;
        private ProfilePictureDatabase _profilePictureDatabase;
        [Inject]
        private void Construct(TitleDatabase titleDatabase, ProfilePictureDatabase profilePictureDatabase)
        {
            _titleDatabase = titleDatabase;
            _profilePictureDatabase = profilePictureDatabase;
        }

        public SaveSystem() 
        {
            _cloudSaveDwonloadedBinding = new EventBinding<CloudSaveDownloaded>(ApplyCloudSave);
            _cloudSaveDataRequestedBinding = new EventBinding<CloudSaveDataRequested>(OnDataRequested);
            _localSaveRequestedBinding = new EventBinding<LocalSaveRequested>(OnSaveRequested);
            _localDataRequestedBinding = new EventBinding<UserDataRequested>(OnUserDataRequested);
            _achievemntStateChanged = new EventBinding<AchievementStateChanged>(OnAchievementStateChanged);
            
            EventBus<CloudSaveDownloaded>.Register(_cloudSaveDwonloadedBinding);
            EventBus<CloudSaveDataRequested>.Register(_cloudSaveDataRequestedBinding);
            EventBus<LocalSaveRequested>.Register(_localSaveRequestedBinding);
            EventBus<UserDataRequested>.Register(_localDataRequestedBinding);
            EventBus<AchievementStateChanged>.Register(_achievemntStateChanged);
        }

        public async UniTask ClearLocalData()
        {
            _localContainer = new SaveContainer();
            _localContainer.Meta.dirty = true;
            _isReady = false;

            foreach (ISaveable savable in _saveables)
            {
                savable.ClearData();
            }
            await SaveLocally();
        }

        public static string SaveFileName()
        {
            string saveFile = Application.persistentDataPath + "/save" + ".json";
            return saveFile;
        }

        public static string AutomaticLoginDataFileName()
        {
            string saveFile = Application.persistentDataPath + "/logincache" + ".json";
            return saveFile;
        }

        public void Register(ISaveable saveable)
        {
            if (!_saveables.Contains(saveable))
            {
                Debug.Log($"Registering locally {saveable.GetType()}...");
                _saveables.Add(saveable);
            }
        }

        public void Unregister(ISaveable saveable)
        {
            if (_saveables.Contains(saveable))
            {
                _saveables.Remove(saveable);
            }
        }

        private void OnAchievementStateChanged(AchievementStateChanged _)
        {
            RequestSave();
        }

        public async UniTask SaveLocally()
        {
            if (!_localContainer.Meta.dirty)
                return;

            Debug.Log("Saving locally...");
            HandleSaveData();

            string saveFilePath = SaveFileName();
            string directory = Path.GetDirectoryName(saveFilePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _localContainer.Meta.lastSaveUtc = Now();
            _localContainer.Meta.dirty = false;

            await File.WriteAllTextAsync(saveFilePath, JsonUtility.ToJson(_localContainer, true));
        }

        public async UniTask SaveLoginCache(AutomaticLoginData data)
        {
            Debug.Log("Saving login cache locally...");

            _automaticLoginData = data;

            string saveFilePath = AutomaticLoginDataFileName();
            string directory = Path.GetDirectoryName(saveFilePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Debug.Log($"[SAVE] Persistent login set to {_automaticLoginData.IsPersistentLogin}");
            await File.WriteAllTextAsync(saveFilePath, JsonUtility.ToJson(_automaticLoginData, true));
        }

        private void HandleSaveData()
        {
            foreach (ISaveable savable in _saveables)
            {
                savable.SaveData(ref _localContainer);
            }
        }

        public async UniTask WaitUntilReady()
        {
            while (!_isReady)
            {
                //Debug.Log("[SAVE] Not ready!!!");
                await UniTask.Yield();
            }
        }

        public async UniTask Load()
        {
            string saveFilePath = SaveFileName();

            if (!File.Exists(saveFilePath))
            {
                Debug.Log("Save file not found, creating default save.");
                _localContainer = new SaveContainer(); 
                _localContainer.Meta = new SaveMeta{ dirty = true };
                await SaveLocally();
            }

            string savedContent = await File.ReadAllTextAsync(saveFilePath);

            _localContainer = JsonUtility.FromJson<SaveContainer>(savedContent);
            await HandleLoadData();
        }

        public async UniTask<AutomaticLoginData> LoadLoginCache()
        {
            string saveFilePath = AutomaticLoginDataFileName();

            if (!File.Exists(saveFilePath))
            {
                Debug.Log("Save file not found, creating default save.");
                _automaticLoginData = new AutomaticLoginData();
                await SaveLoginCache(_automaticLoginData);
            }

            string savedContent = await File.ReadAllTextAsync(saveFilePath);

            _automaticLoginData = JsonUtility.FromJson<AutomaticLoginData>(savedContent);

            Debug.Log($"[SAVE] Persistent login loaded as {_automaticLoginData.IsPersistentLogin}");
            return _automaticLoginData;
        }

        private async UniTask HandleLoadData()
        {
            foreach (ISaveable saveable in _saveables)
            {
                await saveable.LoadData(_localContainer);
            }
        }

        /// <summary>
        /// On succesful cloud data download, save locally
        /// </summary>
        /// <param name="downloaded"></param>
        private async void ApplyCloudSave(CloudSaveDownloaded downloaded)
        {
            _isReady = false;
            string saveFilePath = SaveFileName();

            _localContainer.Identity.IsGuest = downloaded.IsGuest;

            if (!File.Exists(saveFilePath))
            {
                // Create a fully initialized container
                _localContainer = new SaveContainer();

                _localContainer.Identity.IsGuest = downloaded.IsGuest;
                // Merge cloud data into it (cloud may be incomplete)
                _localContainer = Merge(_localContainer, downloaded.CloudData);
            }
            else
            {
                // Local exists merge normally
                _localContainer = Merge(_localContainer, downloaded.CloudData);
            }

            Debug.Log($"[CLOUD] Download {downloaded.CloudData}");
            await HandleLoadData();
            RequestSave();
            RaiseCloudUploadReady();
            OnUserDataRequested(new UserDataRequested());
            _isReady = true;
            Debug.Log("[SAVE] ready!!!");
        }

        private void OnDataRequested(CloudSaveDataRequested request)
        {
            Debug.Log($"[CLOUD] OnDataRequested: {request.IsGuest}");
            RaiseCloudUploadReady(request.FileName,
                            request.Provider,
                            _localContainer
                );
        }

        private async void OnUserDataRequested(UserDataRequested request)
        {
            if (_localContainer == null)
            {
                Debug.LogError("SaveSystem: _localContainer is NULL");
                return;
            }

            if (_localContainer.Identity == null)
            {
                Debug.LogError("SaveSystem: Identity is NULL");
                return;
            }

            if ( _titleDatabase == null)
            {
                Debug.LogError("SaveSystem: _titleDatabase is NULL");
                return;
            }

            var identity = _localContainer.Identity;
            var config = _titleDatabase.GetById(identity.CurrentTitleId);
            if (config == null)
            {
                Debug.LogError($"SaveSystem: Title config NOT FOUND for ID '{_localContainer.Identity.CurrentTitleId}'");
                identity.CurrentTitleId = "NEWCOMER";
                await SaveLocally(); 
                DebounceCloudUpload();
                config = _titleDatabase.GetById(identity.CurrentTitleId);
            }

            var profilePicture = _profilePictureDatabase.Pictures.Find(p => p.Id == (_localContainer.ProfilePictures.SelectedPictureId));
            if (profilePicture == null)
            { 
                Debug.LogError($"SaveSystem: Profile config NOT FOUND for ID '{_localContainer.Identity.CurrentTitleId}'");
            }

            Debug.Log($"[USERDATA] OnDataRequested:" +
                $"Username {identity.Username}, Level {identity.Level}. " +
                $"CurrentTitleId {identity.CurrentTitleId}, " +
                $"TMP_FontMaterial {config.TMP_FontMaterial}" +
                $"CurrentXp {identity.CurrentXp}");

            EventBus<UserDataReady>.Raise(new UserDataReady
            {
                IsGuest = identity.IsGuest,
                Username = identity.Username,
                Level = identity.Level,
                Title = config.DisplayName,
                TitleMaterial = config.TMP_FontMaterial,
                TitleColor = config.TitleColor,
                ProfilePicture = profilePicture.Image,
                CurrentXp = identity.CurrentXp, 
            });
        }

/*        private async void OnUsernameAccepted(UsernameAccepted accepted)
        {
            _localContainer.Identity.Username = accepted.Username;
            await SaveLocally();
            RaiseCloudUploadReady();
            OnUserDataRequested(new UserDataRequested());
        }*/
        
        private void OnSaveRequested(LocalSaveRequested requested)
        {
            RequestSave();
        }

        public void RequestSave()
        {
            MarkDirty();
            DebounceLocalSave();
        }

        private void MarkDirty()
        {
            _localContainer.Meta.dirty = true;
        }

        private void DebounceLocalSave()
        {
            _localSaveCts?.Cancel();
            _localSaveCts?.Dispose();

            _localSaveCts = new CancellationTokenSource();
            LocalSaveDebounced(_localSaveCts.Token).Forget();
        }

        private async UniTaskVoid LocalSaveDebounced(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(400, cancellationToken: token);

                await SaveLocally();

                // AFTER local save settles  schedule cloud upload
                DebounceCloudUpload();
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        private void DebounceCloudUpload()
        {
            if(_cloudUploadInProgress)
                return;
            
            _cloudUploadCts?.Cancel();
            _cloudUploadCts?.Dispose();
            _cloudUploadCts = new CancellationTokenSource();
            CloudUploadDebounced(_cloudUploadCts.Token).Forget();
        }

        private async UniTaskVoid CloudUploadDebounced(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(1500, cancellationToken: token);

                if (_cloudUploadInProgress)
                    return;

                _cloudUploadInProgress = true;
                RaiseCloudUploadReady();
                _cloudUploadInProgress = false;
            }
            catch (OperationCanceledException)
            {
                // Another save happened — restart debounce
            }
        }

        private static void RaiseCloudUploadReady(string fileName = null, CloudProviderType provider = CloudProviderType.Eos, SaveContainer data = null)
        {
            EventBus<CloudSaveUploadReady>.Raise(new CloudSaveUploadReady
            {
                FileName = StringConstants.SaveFileName,
                Provider = CloudProviderType.Eos,
                Data = _localContainer
            });
        }

        private SaveContainer Merge(SaveContainer local, SaveContainer cloud)
        {
            bool isGuest = local.Identity.IsGuest;
            bool isTransferred = local.Identity.AccountTransfered;
            long isTransferredUtc = local.Identity.lastUpdatedUtc;
            SaveContainer result = local;

            // Identity: cloud always wins
            result.Identity = cloud.Identity ?? local.Identity;

            // If login says guest, force guest
            result.Identity.IsGuest = isGuest;
            Debug.Log($"[CLOUD] Merge result.Identity.IsGuest: {result.Identity.IsGuest}");
            result.Identity.AccountTransfered = cloud.Identity.AccountTransfered ? cloud.Identity.AccountTransfered : isTransferred;


            result.Identity.Username = local.Identity.Username ?? cloud.Identity.Username;

            // Wallet: newest wins
            /*            result.Wallet =
                            cloud.Wallet.lastUpdatedUtc > local.Wallet.lastUpdatedUtc
                                ? cloud.Wallet
                                : local.Wallet;*/
            // EOS always wins if it has a value
            result.Wallet = cloud.Wallet ?? local.Wallet;

            // Settings: local device wins
            result.Settings = local.Settings;

            // Modes: newest wins
            result.SpatialMode =
                cloud.SpatialMode.lastUpdatedUtc > local.SpatialMode.lastUpdatedUtc
                    ? cloud.SpatialMode
                    : local.SpatialMode;

            result.ColorMemoryMode =
                cloud.ColorMemoryMode.lastUpdatedUtc > local.ColorMemoryMode.lastUpdatedUtc
                    ? cloud.ColorMemoryMode
                    : local.ColorMemoryMode;

            // Meta
            result.Meta.dirty = false;
            result.Meta.lastSaveUtc = Now();

            for (int i = 0; i < result.Achievements.Count; i++)
            {
                var id = result.Achievements[i].AchievementId;

                var cloudAch = cloud.Achievements.FirstOrDefault(a => a.AchievementId == id);
                var localAch = local.Achievements.FirstOrDefault(a => a.AchievementId == id);

                if (cloudAch == null) Debug.Log("[SAVE] cloudAch is null");
                if (cloudAch == null && localAch == null)
                    continue;

                var isPinned = cloudAch?.lastUpdatedUtc > localAch.lastUpdatedUtc
                    ? cloudAch?.IsPinned
                    : localAch.IsPinned;

                var isRewardClaimed = cloudAch?.lastUpdatedUtc > localAch.lastUpdatedUtc
                    ? cloudAch?.IsRewardClaimed
                    : localAch.IsRewardClaimed;
                // pick the newest
                /*                var newest = cloudAch?.lastUpdatedUtc > localAch?.lastUpdatedUtc
                                    ? cloudAch
                                    : localAch;*/

                result.Achievements[i] = cloudAch?? localAch;
                result.Achievements[i].IsPinned = (bool)isPinned;
                result.Achievements[i].IsRewardClaimed = (bool)isRewardClaimed;
            }

            // Titles: merge by TitleId, newest wins
            {
                var merged = new Dictionary<string, TitleSaveData>();

                // Add local titles first
                foreach (var t in local.TitleSaveData)
                    merged[t.TitleId] = t;

                // Merge cloud titles
                foreach (var t in cloud.TitleSaveData)
                {
                    if (!merged.TryGetValue(t.TitleId, out var existing))
                    {
                        // Not present locally  add it
                        merged[t.TitleId] = t;
                    }
                    else
                    {
                        // Present on both pick newest
                        merged[t.TitleId] =
                            t.lastUpdatedUtc > existing.lastUpdatedUtc
                                ? t
                                : existing;
                    }
                }

                result.TitleSaveData = merged.Values.ToList();
            }

            result.PlayerShipData = cloud.PlayerShipData;

            return result;
        }
        static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    }
}
