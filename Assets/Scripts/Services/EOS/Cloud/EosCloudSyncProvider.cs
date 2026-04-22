using System;
using UnityEngine;
using Data;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using PlayEveryWare.EpicOnlineServices.Samples;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.PlayerDataStorage;

namespace Assets.Scripts.Services.EOS
{
    public class EosCloudSyncProvider : ICloudSyncProvider
    {
        public CloudProviderType ProviderType => CloudProviderType.Eos;

        public UniTask<SaveContainer> Download(string fileName)
        {
            var tcs = new UniTaskCompletionSource<SaveContainer>();

            PlayerDataStorageService.Instance.DownloadFile(fileName, () =>
            {
                try
                {
                    string localCache = PlayerDataStorageService.Instance.GetCachedFileContent(fileName);

                    if (string.IsNullOrEmpty(localCache))
                    {
                        tcs.TrySetResult(null);
                        return;
                    }

                    SaveContainer data = JsonUtility.FromJson<SaveContainer>(localCache);
                    tcs.TrySetResult(data);
                }
                catch(Exception ex) 
                {
                    tcs.TrySetResult(null);
                    Debug.LogError($"Failed to download {fileName}. Ex: {ex}");
                }
            });
            return tcs.Task;
        }

        public UniTask<bool> Exists(string fileName)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null) return UniTask.FromResult(false);

            QueryFileOptions queryFileOptions = new QueryFileOptions { Filename = fileName, LocalUserId = localUserId };

            EOSManager.Instance.GetPlayerDataStorageInterface().QueryFile(
                ref queryFileOptions, null, (ref QueryFileCallbackInfo data) =>
                {
                    if (data.ResultCode == Result.Success)
                    {
                        Debug.Log($"[CLOUD] Succesfully found file with name: {fileName} exists.");
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        Debug.Log($"[CLOUD] File with name: {fileName} does not exist, Error: {data.ResultCode}");
                        tcs.TrySetResult(false);
                    }
                }
            );

            return tcs.Task;
        }

        public UniTask Upload(string fileName, SaveContainer data)
        {
            var tcs = new UniTaskCompletionSource();

            PlayerDataStorageService.Instance.AddFile(fileName, JsonUtility.ToJson(data), () =>
            {
                Debug.Log($"[CLOUD] Succesfully uploaded file with name: {fileName} exists.");
                tcs.TrySetResult();
            });

            return tcs.Task;
        }
    }
}
