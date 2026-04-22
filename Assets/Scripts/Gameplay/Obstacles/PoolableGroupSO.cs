using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Obstacles
{
    public abstract class PoolableGroupSO : ScriptableObject
    {
        public bool IsMixedPool;
        public string ParentName;
        public abstract UniTask SetupPools(IObjectPoolManager poolManager);
    }
}
