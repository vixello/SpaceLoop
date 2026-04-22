using Assets.Scripts.Data;
using Data;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IProfileEditorManager : ISaveable
    {
        public ProfilePictureSaveData ProfilePictures { get; }
        public List<ProfilePictureConfig> GetDatabaseData();
        public ProfilePictureSaveData GetPictureSavedData();
        public string GetSavedUsername();
        public void ProfileChanged(string selectedId, string username);
    }

}
