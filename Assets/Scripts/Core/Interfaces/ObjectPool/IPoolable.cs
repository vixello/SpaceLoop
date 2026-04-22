using Assets.Scripts.Data.ObjectPool;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IPoolContext
    {

    }
    public interface IPoolable<T> where T : MonoBehaviour, IPoolable<T>
    {
        IObjectPool<T> ObjectPool { set; }
        public PoolSettings PoolSettings { get; }
        void SetPool(IObjectPool<T> pool);
        public delegate void PoolableObjectCollision(IObjectPool<T> objectPool);
        public event PoolableObjectCollision OnPoolableObjectCollision;

        //public TContext GetContext();
    }
}
