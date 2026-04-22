using Cysharp.Threading.Tasks;
using Data;

namespace Assets.Scripts.Core.Interfaces
{
    public enum CloudProviderType
    {
        Eos
    }
    public enum CloudSyncReason
    {
        AppStart,
        Login,
        ProfileOpened,
        MainMenuOpened,
        ManualRequest,
        ConnectivityRestored,
        SaveChanged,
        AccountSwitch,
        FailedDownload
    }

    public interface ICloudSyncProvider
    {
        CloudProviderType ProviderType { get; }
        public UniTask<bool> Exists(string fileName);
        public UniTask Upload(string fileName, SaveContainer data);
        public UniTask<SaveContainer> Download(string fileName);
    }

    public interface ICloudSyncProviderFactory
    {
        public Assets.Scripts.Core.Interfaces.ICloudSyncProvider GetCloud(CloudProviderType cloudProvider);
    }
}
