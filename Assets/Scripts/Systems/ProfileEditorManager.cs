using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data;
using Cysharp.Threading.Tasks;
using Data;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Systems
{
    public class ProfileEditorManager : IProfileEditorManager
    {
        private ProfilePictureDatabase _database;

        private ProfilePictureSaveData _savedPictures = new ProfilePictureSaveData();
        public ProfilePictureSaveData ProfilePictures => _savedPictures;
        private IIdentityManager _identityManager;

        private string _newUsername;
        private EventBinding<UsernameAccepted> _usernameAcceptedBinding;

        [Inject]
        private void Construct(ProfilePictureDatabase database, IIdentityManager identityManager)
        {
            _database = database;
            _identityManager = identityManager;
        }

        public ProfileEditorManager()
        {
            _usernameAcceptedBinding = new EventBinding<UsernameAccepted>(OnUsernameAccepted);
            EventBus<UsernameAccepted>.Register(_usernameAcceptedBinding);

        }

        public bool IsUnlocked(string id) => _savedPictures.UnlockedPictureIds.Contains(id);

        public ProfilePictureConfig GetPicture(string id)
            => _database.Pictures.Find(p => p.Id == id);

        public void UnlockPicture(string id)
        {
            if (_savedPictures.UnlockedPictureIds.Contains(id)) return;
            _savedPictures.UnlockedPictureIds.Add(id);
            EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested());
        }

        public void SaveData(ref SaveContainer data)
        {
            if (_savedPictures == null)
            {
                Debug.LogError("_savedPictures is NULL");
                return;
            }

            if (_savedPictures.UnlockedPictureIds == null)
            {
                Debug.LogWarning("_savedPictures.UnlockedPictureIds was NULL, creating new list");
                _savedPictures.UnlockedPictureIds = new List<string>();
            }

            if (data.ProfilePictures.UnlockedPictureIds == null)
            {
                Debug.LogWarning("data.ProfilePictures.UnlockedPictureIds was NULL, creating new list");
                data.ProfilePictures.UnlockedPictureIds = new List<string>();
            }

            data.Identity.Username = string.IsNullOrEmpty(_newUsername) ? data.Identity.Username : _newUsername;

            var unlockedInSave = data.ProfilePictures.UnlockedPictureIds;

            foreach (var pic in _savedPictures.UnlockedPictureIds)
            {
                if (!unlockedInSave.Contains(pic))
                    unlockedInSave.Add(pic);
            }

            EventBus<ProfileChanged>.Raise(new ProfileChanged());
            _newUsername = "";
        }


        public UniTask LoadData(SaveContainer data)
        {
            _savedPictures = new ProfilePictureSaveData();

            if (data.ProfilePictures != null)
            {
                Debug.Log($"LoadData pic data");
                _savedPictures = data.ProfilePictures;

                if(_savedPictures.UnlockedPictureIds.Count == 0)
                {
                    _savedPictures.UnlockedPictureIds.Add("picture_01");

                }
                if (string.IsNullOrEmpty(_savedPictures.SelectedPictureId))
                {
                    _savedPictures.SelectedPictureId = "picture_01";
                }
                RevaluateUnlockedPictures(data.Identity.Level);
            }
            return UniTask.CompletedTask;
        }

        public void ClearData()
        {
            _savedPictures = new ProfilePictureSaveData();
            _newUsername = "";
        }

        public void RevaluateUnlockedPictures(int playerLevel)
        {
            bool changed = false;

            foreach (var pic in _database.Pictures)
            {
                if (pic == null) continue;
                if (pic.LevelToUnlock <= playerLevel)
                {
                    if (!_savedPictures.UnlockedPictureIds.Contains(pic.Id))
                    {
                        _savedPictures.UnlockedPictureIds.Add(pic.Id);
                        changed = true;
                    }
                }
            }
            if(changed)
            {
                EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested { });
            }
        }

        public List<ProfilePictureConfig> GetDatabaseData()
        {
            return _database.Pictures;  
        }

        public ProfilePictureSaveData GetPictureSavedData()
        {
            return _savedPictures;
        }

        public string GetSavedUsername()
        {
            return _identityManager.GetIdentity().Username;
        }

        public void ProfileChanged(string selectedId, string username)
        {
            Debug.Log($"ProfileChanged {selectedId} {username}");
            _savedPictures.SelectedPictureId = selectedId;
            _newUsername = username?? _newUsername;
            EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested { });
        }

        private void OnUsernameAccepted(UsernameAccepted accepted)
        {
            _newUsername = accepted.Username ?? _newUsername;
            EventBus<LocalSaveRequested>.Raise(new LocalSaveRequested { });
        }
    }
}
