using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using Data;

namespace Assets.Scripts.Gameplay
{
    public class PlayerSpaceshipManager : ISaveable
    {
        private PlayerShipData _data = new PlayerShipData();

        public string GetSelectedSpaceship()
        {
            return _data.Selected;
        }

        public void ClearData()
        {
            _data = new PlayerShipData();
        }

        public UniTask LoadData(SaveContainer data)
        {
            if (data == null) return UniTask.CompletedTask;

            _data = data.PlayerShipData;
            return UniTask.CompletedTask;   
        }

        public void SaveData(ref SaveContainer data)
        {
            if (data.PlayerShipData == null)
                data.PlayerShipData = new PlayerShipData();
            else
            {
                data.PlayerShipData = _data;
            }
        }
    }
}
