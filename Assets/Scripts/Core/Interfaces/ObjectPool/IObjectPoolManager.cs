using Assets.Scripts.Data.ObjectPool;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IObjectPoolManager
    {
        void AddChildrenToPool<T>(GameObject parent, bool isMixedPool)
            where T : MonoBehaviour, IPoolable<T>;

        IObjectPool<T> SetupPoolSeparatePrefab<T>(T prefab)
            where T : MonoBehaviour, IPoolable<T>;
    }

    public enum ObstacleType
    {
        Basic
    }

    public struct ObstacleContext : IEquatable<ObstacleContext>
    {
        public ObstacleType ObstacleType;
        public bool IsThemeExclusive;
        public bool Equals(ObstacleContext other)
        {
            return ObstacleType == other.ObstacleType &&
                   IsThemeExclusive == other.IsThemeExclusive;
        }

        public override bool Equals(object obj)
        {
            return obj is ObstacleContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + ObstacleType.GetHashCode();
            hash = hash * 31 + IsThemeExclusive.GetHashCode();
            return hash;
        }
    }
}
