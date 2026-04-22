using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data.ObjectPool;
using Assets.Scripts.Gameplay.Obstacles;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Gameplay.ObjectPool
{
    public class ObjectPoolManager : MonoBehaviour, IInitializable, IObjectPoolManager
    {
        private static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance => _instance;

        [Header("Prefabs to pool")]
        [SerializeField] private List<PoolableGroupSO> _poolGroups;

        private Dictionary<Type, IList> _pools = new();
        //private Dictionary<ObstacleContext, IObjectPool<ObstaclePoolable>> _pools = new();

        //internal Dictionary<Type, IList> _activeElements = new();

        private bool _isInitialized = false;
        public bool IsInitialized() => _isInitialized;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /*    private void Start()
            {
                //await Initialize();
                EventBus.Instance.OnCheckObstaclePoolsExistence += SendExistingObstaclePools;
            }

            private void OnDestroy()
            {
                EventBus.Instance.OnCheckObstaclePoolsExistence -= SendExistingObstaclePools;
            }*/

        public async UniTask Initialize()
        {
            //foreach (PoolableBackgroundElement bgElementPrefab in _bgElementPrefabs)
            //{
            //    SetupPoolSeparatePrefab(bgElementPrefab);
            //}

            //foreach (ObstaclePoolable obstaclePrefab in _obstaclePrefabs)
            //{
            //    SetupPoolSeparatePrefab(obstaclePrefab);
            //}

            //AddChildrenToPool<PoolableBackgroundElement>(_backgroundParentGameObject, false);
            //AddChildrenToPool<ObstaclePoolable>(_obstacleParentGameObject, false);

            foreach (var group in _poolGroups)
            {
                await group.SetupPools(this);
            }

            _isInitialized = true;
            await Task.Yield();
        }


        //ADD ARLEADY SPAWNED OBJECTS TO THEIR RESPECTIVE POOLS BY TYPE or TO A MIXED POOL
        public void AddChildrenToPool<T>(GameObject parentGameObject, bool isMixedPool) where T : MonoBehaviour, IPoolable<T>
        {
            int poolIndex = 0;

            foreach (Transform child in parentGameObject.transform)
            {
                T elementInstance = child.GetComponent<T>();

                if (elementInstance != null)
                {
                    Type elementType = typeof(T);

                    // Fetch the list of pools for this type
                    var poolList = _pools[elementType] as List<IObjectPool<T>>;
                    if (poolList != null && poolList.Count > poolIndex)
                    {
                        elementInstance.ObjectPool = (IObjectPool<T>)poolList[poolIndex];
                        Debug.Log("Added " + elementType);
                        AddToActiveElements(elementInstance);
                    }
                    else
                    {
                        Debug.LogWarning($"Pool list for {elementType} is null or index {poolIndex} out of range.");
                    }
                    if (!isMixedPool) poolIndex++; //IF POOL IS MIXED, STAY AT THE SAME INDEX
                }
            }
        }

        public IObjectPool<T> SetupPoolSeparatePrefab<T>(T prefab) where T : MonoBehaviour, IPoolable<T>
        {
            Type poolType = typeof(T);

            if (!_pools.ContainsKey(poolType))
            {
                _pools[poolType] = new List<IObjectPool<T>>();
            }

            var poolList = (List<IObjectPool<T>>)_pools[poolType];
            int index = poolList.Count;

            PoolSettings poolSettings = prefab.PoolSettings;
            if (poolSettings == null)
            {
                Debug.Log($"[POOL MANAGER] poolSettings is null for : {prefab.GetType().Name} ");
            }

            Debug.Log($"[POOL MANAGER] Creating pool for {prefab.GetType().Name}");

            IObjectPool<T> pool = new UnityEngine.Pool.ObjectPool<T>(
                () => CreatePoolElementInstance(prefab, index),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPooledObject,
                poolSettings.GetCollectionCheck(),
                poolSettings.GetDefaultCapacity(),
                poolSettings.GetMaxSize()
                );


            poolList.Add(pool);
            return pool;
        }

        private void SetupPoolMultiplePrefabs<T>(T[] prefabs) where T : MonoBehaviour, IPoolable<T>
        {
            Type poolType = typeof(T);

            if (!_pools.ContainsKey(poolType))
            {
                _pools[poolType] = new List<IObjectPool<T>>();

                var poolList = (List<IObjectPool<T>>)_pools[poolType];
                PoolSettings poolSettings = prefabs[0].PoolSettings;


                IObjectPool<T> pool = new UnityEngine.Pool.ObjectPool<T>(
                    () => CreateRandomPoolElementInstance(prefabs),
                    OnGetFromPool,
                    OnReleaseToPool,
                    OnDestroyPooledObject,
                    poolSettings.GetCollectionCheck(),
                    poolSettings.GetDefaultCapacity(),
                    poolSettings.GetMaxSize()
                    );

                poolList.Add(pool);
            }
        }
        private static Dictionary<Type, IList> _activeElements = new();
        public static void SetActiveElements(Dictionary<Type, IList> activeElements) => _activeElements = activeElements;
        public static Dictionary<Type, IList> GetActiveObjectPoolElements()
        {
            if (_activeElements == null)
                _activeElements = new Dictionary<Type, IList>();
            return _activeElements;
        }

        private void ModifyActiveElements<T>(T element, bool add) where T : MonoBehaviour, IPoolable<T>
        {
            Dictionary<Type, IList> activeElements = GetActiveObjectPoolElements();
            var elementType = typeof(T);

            if (!activeElements.TryGetValue(elementType, out var list))
            {
                list = new List<T>();
                activeElements[elementType] = list;
            }

            if (add)
            {
                if (!list.Contains(element))
                    list.Add(element);
            }
            else
            {
                list.Remove(element);
            }

            SetActiveElements(activeElements);
        }

        private void AddToActiveElements<T>(T element) where T : MonoBehaviour, IPoolable<T>
            => ModifyActiveElements(element, true);

        private void RemoveFromActiveElements<T>(T element) where T : MonoBehaviour, IPoolable<T>
            => ModifyActiveElements(element, false);

        private T CreatePoolElementInstance<T>(T prefab, int index) where T : MonoBehaviour, IPoolable<T>
        {
            T poolElemenetInstance = Instantiate(prefab);
            var poolList = _pools[typeof(T)] as List<IObjectPool<T>>;
            poolElemenetInstance.SetPool(poolList[index]);

            return poolElemenetInstance;
        }

        private T CreateRandomPoolElementInstance<T>(T[] obstaclePrefabs) where T : MonoBehaviour, IPoolable<T>
        {
            T randomPrefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
            T poolElemenetInstance;

            if (randomPrefab != null)
            {
                poolElemenetInstance = Instantiate(randomPrefab);
                var poolList = _pools[typeof(T)] as List<IObjectPool<T>>;
                poolElemenetInstance.SetPool(poolList[0]);
                return poolElemenetInstance;
            }

            poolElemenetInstance = Instantiate(obstaclePrefabs[0]);
            return poolElemenetInstance;
        }

        private void OnGetFromPool<T>(T element) where T : MonoBehaviour, IPoolable<T>
        {
            if (element != null)
            {
                AddToActiveElements(element);
                element.gameObject.SetActive(true);
                //EventBus.Instance.InvokeThemeChangeForObject(element);
            }
        }

        private void OnReleaseToPool<T>(T element) where T : MonoBehaviour, IPoolable<T>
        {
            element.gameObject.SetActive(false);
        }

        private void OnDestroyPooledObject<T>(T element) where T : MonoBehaviour, IPoolable<T>
        {
            RemoveFromActiveElements<T>(element);
            Destroy(element.gameObject);
        }

        public IObjectPool<T> GetPoolOfType<T>() where T : MonoBehaviour, IPoolable<T>
        {
            if (_pools.ContainsKey(typeof(T)))
            {
                var poolList = _pools[typeof(T)] as List<IObjectPool<T>>;
                return poolList[0];
            }
            return null;
        }


        //public void SendExistingObstaclePools()
        //{
        //    if (_pools != null)
        //        EventBus.Instance.InvokeCheckedObstaclePoolsExistence(_pools);
        //}
    }

}
