using Assets.Scripts.Core;
using UnityEngine;
using Data;
using Core;
using VContainer;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using System;

namespace Assets.Scripts.Services.Cloud
{
    public class CloudSyncService : IInitializable
    {
        private bool _isInitialized = false;
        private bool _downloadFailed = false;
        private bool _isGuest;
        public bool IsInitialized() => _isInitialized;

        private ICloudSyncProviderFactory _cloudSyncProviderFactory;
        private EventBinding<CloudSaveDownloadFailed> _saveDownloadFailedBinding;
        private EventBinding<LoginSucceded> _loginBinding;
        private EventBinding<CloudSaveUploadReady> _onDataReceivedBinding;

        [Inject]
        private void Construct(ICloudSyncProviderFactory cloudSyncProviderFactory)
        {
            _cloudSyncProviderFactory = cloudSyncProviderFactory;

            _saveDownloadFailedBinding = new EventBinding<CloudSaveDownloadFailed>(OnDownloadFailed);
            _loginBinding = new EventBinding<LoginSucceded>(OnLoginSucceded);
            _onDataReceivedBinding = new EventBinding<CloudSaveUploadReady>(OnDataReceived);
            EventBus<CloudSaveDownloadFailed>.Register(_saveDownloadFailedBinding);
            EventBus<LoginSucceded>.Register(_loginBinding);
            EventBus<CloudSaveUploadReady>.Register(_onDataReceivedBinding);

        }

        public UniTask Initialize()
        {
            _isInitialized = true;
            return UniTask.CompletedTask;
        }

        public async UniTask DownloadFromCloud(string fileName, CloudProviderType cloudProvider)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return;

            ICloudSyncProvider cloudSyncProvider = await GetSyncProvider(cloudProvider);

            if (cloudSyncProvider == null)
            {
                Debug.LogError("[CLOUD] Provider is null");
                return;
            }

            if (!await cloudSyncProvider.Exists(fileName)) {
                _downloadFailed = true;
                EventBus<CloudSaveDownloadFailed>.Raise(new CloudSaveDownloadFailed { FileName = fileName });
                return;
            }

            var data = await cloudSyncProvider.Download(fileName);
            if(data == null)
            {
                _downloadFailed = true;
                EventBus<CloudSaveDownloadFailed>.Raise(new CloudSaveDownloadFailed { FileName = fileName });
                return;
            }
            /*            
                        if (data == null)
                        {
                            EventBus<CloudSaveDownloadFailed>.Raise(new CloudSaveDownloadFailed { FileName = FileName });
                            return;
                        }*/
            EventBus<CloudSaveDownloaded>.Raise(new CloudSaveDownloaded { CloudData = data, IsGuest = _isGuest});
            _downloadFailed = false; ;
        }

        private UniTask<ICloudSyncProvider> GetSyncProvider(CloudProviderType cloudProviderType)
        {
            return UniTask.FromResult<ICloudSyncProvider>(_cloudSyncProviderFactory.GetCloud(cloudProviderType));
        }

        private void OnDownloadFailed(CloudSaveDownloadFailed failed)
        {
            Debug.LogError($"[CLOUD] Download failed {failed.FileName} _isGuest {_isGuest}");

            EventBus<CloudSaveDataRequested>.Raise(
                    new CloudSaveDataRequested
                    {
                        IsGuest = _isGuest,
                        FileName = failed.FileName,
                        Provider = CloudProviderType.Eos,
                        Reason = CloudSyncReason.ConnectivityRestored,
                    });
        }

        private async void OnLoginSucceded(LoginSucceded succeded)
        {
            _isGuest = succeded.IsGuest;    
            Debug.Log($"[AUTH] OnLoginSucceded ");
            await RequestSync(succeded.SyncReason, succeded.CloudProviderType);
        }

        private async void OnDataReceived(CloudSaveUploadReady ready)
        {
            Debug.Log($"[CLOUD] OnDataReceived");
            await UploadToCloud(ready.FileName, ready.Data, ready.Provider);

            if (_downloadFailed)
            {
                Debug.Log("[CLOUD] Retrying download...");
                await RequestSync(CloudSyncReason.FailedDownload, ready.Provider);
            }
        }

        public async UniTask RequestSync(CloudSyncReason syncReason, CloudProviderType cloudProviderType)
        {
            Debug.Log($"[CLOUD] RequestSync save");
            await DownloadFromCloud("save", cloudProviderType);
        }

        public async UniTask UploadToCloud(string fileName, SaveContainer data, CloudProviderType cloudProviderType)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return;

            ICloudSyncProvider cloudSyncProvider = await GetSyncProvider(cloudProviderType);

            await cloudSyncProvider.Upload(fileName, data);
            // activate an event to the eos player data logic to upload
        }
    }
}
