using Cysharp.Threading.Tasks;
using Data;

namespace Assets.Scripts.Core.Interfaces
{

    public interface ISaveable
    {
        void SaveData(ref SaveContainer data);
        UniTask LoadData(SaveContainer data);
        void ClearData();
    }
}
