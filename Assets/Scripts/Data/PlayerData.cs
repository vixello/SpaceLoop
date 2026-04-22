using System;
using System.Collections.Generic;

namespace Data
{
    public abstract class SyncData
    {
        public long lastUpdatedUtc;
    }

    /// <summary>
    /// A container used to save the entirety of the player data
    /// </summary>
    [System.Serializable]
    public class SaveContainer
    {
        public SaveMeta Meta = new SaveMeta();
        public IdentityData Identity = new IdentityData();
        public WalletData Wallet = new WalletData();
        public ModeLightsSpatialInfiniteData SpatialMode = new ModeLightsSpatialInfiniteData();
        public ModeLightsColorMemoryData ColorMemoryMode = new ModeLightsColorMemoryData();
        public SettingsData Settings = new SettingsData();
        public List<LocalAchievementSaveData> Achievements = new List<LocalAchievementSaveData>();
        public ProfilePictureSaveData ProfilePictures = new ProfilePictureSaveData();
        public List<TitleSaveData> TitleSaveData = new List<TitleSaveData>();
        public PlayerShipData PlayerShipData = new PlayerShipData();
    }

    [Serializable]
    public class PlayerShipData
    {
        public string Selected;
        public List<string> Unlocked;
    }

    [Serializable]
    public class SaveMeta
    {
        public long lastSaveUtc;
        public string saveVersion;
        public bool dirty;
    }

    [Serializable]
    public class IdentityData : SyncData
    {
        public bool IsGuest = true; // if device id is not linked to any identiy provieder
        public bool AccountTransfered = false; 

        public int Level;
        public int CurrentXp;

        public string CurrentTitleId;

        public string EosPuid; // upon linking device with some identity provider, store the puid for silent logi
        public string Username;
    }

    [Serializable]
    public class WalletData : SyncData
    {
        public int coins;
        public int gems;
    }

    [Serializable]
    public class ModeLightsSpatialInfiniteData : SyncData
    {
        public int score;
        public int rank;
    }

    [Serializable]
    public class ModeLightsColorMemoryData : SyncData
    {
        public int score;
        public int rank;
    }

    [Serializable]
    public class SettingsData : SyncData
    {
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
    }
    
    [Serializable]
    public class LocalAchievementSaveData : SyncData
    {
        public string AchievementId;
        public double Progress;
        public bool IsRewardClaimed;
        public bool IsUnlocked;
        public bool IsPinned;
        public int PinOrder = -1;
        public DateTimeOffset? UnlockTime;
        public Dictionary<string, double> StatProgress = new Dictionary<string, double>();
    }

    [Serializable]
    public class ProfilePictureSaveData : SyncData
    {
        public List<string> UnlockedPictureIds = new List<string>();
        public string SelectedPictureId;
    }

    [Serializable]
    public class TitleSaveData : SyncData
    {
        public string TitleId;
        public string RequiredAchievementId;
        public bool IsUnlocked;
        public bool IsSelected;
    }

    [System.Serializable] 
    public class AutomaticLoginData 
    { 
        public bool IsPersistentLogin = false;

    }
}
