using UnityEngine;

namespace Assets.Scripts.Data.ObjectPool
{
    [CreateAssetMenu(fileName = "New PoolSettings", menuName = "Object Pool/PoolSettings")]

    public class PoolSettings : ScriptableObject
    {
        [SerializeField] private bool _collectionCheck;
        [SerializeField] private int _defaultCapacity;
        [SerializeField] private int _maxSize;
        [SerializeField] private float _nextTimeToSpawn;

        public bool GetCollectionCheck() => _collectionCheck;
        public int GetDefaultCapacity() => _defaultCapacity;
        public int GetMaxSize() => _maxSize;
        public float GetNextTimeToSpawn() => _nextTimeToSpawn;
    }
}
