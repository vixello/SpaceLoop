using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Systems;
using Cysharp.Threading.Tasks;
using Data;
using Epic.OnlineServices;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Services.Auth
{

    public class IdentityManager : ISaveable, IIdentityManager
    {
        private readonly ISaveSystem _saveSystem;
        public IdentityData Data { get; private set; }

        [Inject]
        public IdentityManager(ISaveSystem saveSystem) 
        { 
            _saveSystem = saveSystem;
            _saveSystem.Register(this);

            Data = new IdentityData();
        }

        public UniTask LoadData(SaveContainer data)
        {
            if (data.Identity != null) 
            {
                Debug.Log("[AUTH] Loaded data into identity manager " + data.Identity.Username);
                Data = data.Identity;
            }

            return UniTask.CompletedTask;
        }

        public void SaveData(ref SaveContainer data)
        {
            if (Data == null) return;
            data.Identity = Data;
        }
        public void ClearData()
        {
            Data = new IdentityData();
        }

        public bool IsUsernameSet()
        {
            return (!string.IsNullOrEmpty(Data.Username));
        }

        public void StoreLinkedPuid(ProductUserId puid)
        {
            Data.EosPuid = puid.ToString();
            Data.IsGuest = false;
            _saveSystem.RequestSave();
        }

        public void StoreGuestPuid(ProductUserId puid)
        {
            Data.EosPuid = puid.ToString();
            Data.IsGuest = true;
            _saveSystem.RequestSave();
        }

        public void ClearIdentity()
        {
            Data = new IdentityData();
            _saveSystem.RequestSave();
        }

        public IdentityData GetIdentity()
        {
            return Data;
        }
    }
}
