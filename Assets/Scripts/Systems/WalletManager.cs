using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Security.Principal;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Assets.Scripts.Systems
{
    public class WalletManager : ISaveable, IStartable
    {
        private ISaveSystem _saveSystem;
        private WalletData _data = new WalletData();

        [Inject]
        public void Construct(ISaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        void IStartable.Start()
        {
            Debug.Log("wallet");
            _saveSystem.Register(this);
        }

        public UniTask LoadData(SaveContainer data)
        {
            if(data != null)
            {
                if (data.Wallet == null)
                    data.Wallet = new WalletData();

                _data = data.Wallet;
            }

            return UniTask.CompletedTask;
        }

        public void SaveData(ref SaveContainer data)
        {
            Debug.Log("Save data wallet");
            if (_data == null)
                _data = new WalletData(); 

            _data.lastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            data.Wallet = _data;
        }

        public void ClearData()
        {
            _data = new WalletData();
        }
    }
}
