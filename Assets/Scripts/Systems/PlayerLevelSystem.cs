using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data.Level;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Assets.Scripts.Systems
{

    public class PlayerLevelSystem : ISaveable
    {
        private readonly IIdentityManager _identityManager;
        private readonly LevelCurve _levelCurve;
        private IdentityData _identity;

        EventBinding<PlayerXpGained> _playerXpGainedBidning;

        public PlayerLevelSystem(
            IIdentityManager identityManager, // switch to identity manager
            LevelCurve levelCurve //TODO
            )
        {
            _identityManager = identityManager;
            _levelCurve = levelCurve;
            _identity = _identityManager.GetIdentity();

            _playerXpGainedBidning = new EventBinding<PlayerXpGained>(OnXpGained);
            EventBus<PlayerXpGained>.Register(_playerXpGainedBidning);
        }

        private void OnXpGained(PlayerXpGained e)
        {
            Debug.Log("[REWARD] ON xp gained");
            AddXp(e.Amount);
        }

        private void AddXp(int amount)
        {
            _identity.CurrentXp += amount;

            while (_identity.CurrentXp >= RequiredXpForNextLevel())
            {
                _identity.CurrentXp -= RequiredXpForNextLevel();
                LevelUp();
            }
        }

        private int RequiredXpForNextLevel()
        {
            return _levelCurve.GetXpForLevel(_identity.Level);
        }

        private void LevelUp()
        {
            _identity.Level++;

            EventBus<PlayerLevelUp>.Raise(new PlayerLevelUp(_identity.Level));
            EventBus<PlayerTitleChanged>.Raise(new PlayerTitleChanged(_identity.CurrentTitleId));}

        public void SaveData(ref SaveContainer data)
        {
            if (data != null && _identity != null)
            {
                Debug.Log($"[REWARD] SaveData xp {_identity.CurrentXp}");
                data.Identity.Level = _identity.Level;
                data.Identity.CurrentXp = _identity.CurrentXp;
            }
        }

        public UniTask LoadData(SaveContainer data)
        {
            if (data != null && _identity != null)
            {
                _identity.Level = data.Identity.Level == 0 ? 1 : data.Identity.Level;
                _identity.CurrentXp = data.Identity.CurrentXp == 0 ? 1 : data.Identity.CurrentXp;
                _identity.CurrentTitleId = data.Identity.CurrentTitleId;
            }
            return UniTask.CompletedTask;
        }

        public void ClearData()
        {
            _identity = new IdentityData();
        }
    }
}
