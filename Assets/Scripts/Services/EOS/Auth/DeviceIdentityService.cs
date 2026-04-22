using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using System;
using UnityEngine;

namespace Assets.Scripts.Services.EOS.Auth
{
    public class DeviceIdentityService : IDeviceIdentityService
    {
        private ProductUserId _devicePuid;

        public bool HasDeviceId => _devicePuid != null;
        public ProductUserId CurrentDevicePuid => _devicePuid;

        public UniTask<Result> CreateDeviceIdAsync()
        {
            var tcs = new UniTaskCompletionSource<Result>();
            var connect = EOSManager.Instance.GetEOSConnectInterface();

            var opt = new CreateDeviceIdOptions
            {
                DeviceModel = SystemInfo.deviceModel
            };

            connect.CreateDeviceId(ref opt, null,
                (ref CreateDeviceIdCallbackInfo info) =>
                {
                    if (info.ResultCode == Result.Success ||
                        info.ResultCode == Result.DuplicateNotAllowed)
                    {
                        Debug.Log("[AUTH] CreateDeviceIdAsync");
                        tcs.TrySetResult(info.ResultCode);
                    }
                    else
                    {
                        Debug.LogError("[AUTH] CreateDeviceIdAsync failed");
                        tcs.TrySetException(new Exception(info.ResultCode.ToString()));
                    }
                });

            return tcs.Task;
        }

        public UniTask<Result> DeleteDeviceIdAsync()
        {
            var tcs = new UniTaskCompletionSource<Result>();
            var connect = EOSManager.Instance.GetEOSConnectInterface();
            var opt = new DeleteDeviceIdOptions();

            connect.DeleteDeviceId(ref opt, null,
                (ref DeleteDeviceIdCallbackInfo info) =>
                {
                    if (info.ResultCode == Result.Success ||
                        info.ResultCode == Result.NotFound)
                    {
                        Debug.Log("[AUTHSWITCH] DeleteDeviceIdAsync");
                        tcs.TrySetResult(info.ResultCode);
                    }
                    else
                    {
                        Debug.LogError("[AUTHSWITCH] DeleteDeviceIdAsync failed");
                        tcs.TrySetException(new Exception(info.ResultCode.ToString()));
                    }
                });

            return tcs.Task;
        }


        public void SetCurrentDevicePuid(ProductUserId puid)
        {
            _devicePuid = puid;
        }
    }

}
