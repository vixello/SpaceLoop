using Assets.Scripts.Core.Interfaces;
using Data;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public struct CloudSaveDownloaded : IEvent
    {
        public SaveContainer CloudData;
        public bool IsGuest;
    }

    public struct CloudSaveDownloadFailed : IEvent
    {
        public string FileName;
    }

    public struct CloudSaveDataRequested : IEvent
    {
        public bool IsGuest;
        public string FileName;
        public CloudProviderType Provider;
        public CloudSyncReason Reason;
    }

    public struct CloudSaveUploadReady : IEvent
    {
        public string FileName;
        public CloudProviderType Provider;
        public SaveContainer Data;
    }
    public struct CloudSaveDownloadRequested : IEvent
    {
        public string FileName;
        public CloudProviderType Provider;
    }
    
    public struct StatIncrement : IEvent
    {
        public string Name;
    }

    public struct LocalSaveRequested : IEvent { }
    public struct UserDataRequested : IEvent { }
    public struct UserDataReady : IEvent
    {
        public bool IsGuest;
        public int Level;
        public int CurrentXp;
        public string Username;
        public string Title;
        public Material TitleMaterial;
        public Color TitleColor;

        public Sprite ProfilePicture;
    }

    public struct StatIncreaseRequested : IEvent
    {
        public string Name;
        public int Amount;
    }
}
