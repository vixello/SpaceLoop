using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data.ObjectPool;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Gameplay.Obstacles
{
    public class ObstaclePoolGroupSO : PoolableGroupSO
    {
        public ObstaclePoolable[] ObstaclePrefabs;
        private Dictionary<ObstacleContext, IObjectPool<ObstaclePoolable>> _pools = new();

        public override async UniTask SetupPools(IObjectPoolManager poolManager)
        {
            foreach (var prefabGO in ObstaclePrefabs)
            {
                var poolObject = prefabGO.GetComponent<ObstaclePoolable>();
                IObjectPool<ObstaclePoolable> pool = (IObjectPool<ObstaclePoolable>)poolManager.SetupPoolSeparatePrefab<ObstaclePoolable>(poolObject);

                //ObstaclePoolable obstaclePoolable = poolObject as ObstaclePoolable;
                ObstacleContext obstacleData = poolObject.GetContext();

                if (!_pools.ContainsKey(obstacleData))
                {
                    _pools[obstacleData] = pool as IObjectPool<ObstaclePoolable>;
                }
                await Task.Yield();
            }

            GameObject parentObject = GameObject.FindGameObjectWithTag(ParentName);
            poolManager.AddChildrenToPool<ObstaclePoolable>(parentObject, IsMixedPool);


            await Task.Yield();
        }
    }
}
