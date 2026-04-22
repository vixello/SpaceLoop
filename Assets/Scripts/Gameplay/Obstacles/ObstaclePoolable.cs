using UnityEngine;
using UnityEngine.Pool;
using Assets.Scripts.Core.Interfaces;
using static Assets.Scripts.Core.Interfaces.IPoolable<Assets.Scripts.Gameplay.Obstacles.ObstaclePoolable>;
using Assets.Scripts.Data.ObjectPool;

namespace Assets.Scripts.Gameplay.Obstacles
{
    public class ObstaclePoolable : Obstacle, IPoolable<ObstaclePoolable>
    {
        private float _previousObstacleHeight => _obstacleStartPosition.y;
        private IObjectPool<ObstaclePoolable> _objectPool;
        public IObjectPool<ObstaclePoolable> ObjectPool { set => _objectPool = value; }

        [Header("Pool Settings")]
        [SerializeField] private PoolSettings _poolSettings;
        public PoolSettings PoolSettings => _poolSettings;
        public event PoolableObjectCollision OnPoolableObjectCollision;

        protected override void Start()
        {
            OnPoolableObjectCollision += SpawnNextObstacle;

            base.Start();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("ScreenStart"))
            {
                OnPoolableObjectCollision.Invoke(_objectPool);
            }
            base.OnTriggerEnter2D(collision);
        }

        public void SetPool(IObjectPool<ObstaclePoolable> pool)
        {
            if (pool is IObjectPool<ObstaclePoolable> obstaclePool)
            {
                ObjectPool = obstaclePool;
            }
        }

        private void SpawnNextObstacle(IObjectPool<ObstaclePoolable> objectPool)
        {
            //Utils.Instance.SetPreviousObstacleHeight(_previousObstacleHeight);
            //EventBus.Instance.InvokeSpawnNextObstacleFromPool(_noteCheckVFXobjectPool, _obstacleType);
            //EventBus.Instance.InvokeSpawnNextObstacle();
        }

        public override void Deactivate()
        {
            if (_objectPool != null && !_isFirstBatch)
            {
                _objectPool.Release(this);

                _obstacleStartPosition = (Sprite != null) ? new Vector3(_obstacleStartPosition.x, _obstacleBehaviour.SetupObstacleHeight(Sprite.bounds.size.x, transform.position.z).y, _obstacleStartPosition.z) : _obstacleStartPosition;


                gameObject.transform.position = _obstacleStartPosition;
                _obstacleBehaviour.OnDeactivateBehaviour();
            }
            else
            {
                base.Deactivate();
            }
        }


        public ObstacleContext GetContext()
        {
            return new ObstacleContext
            {
                ObstacleType = _obstacleType,
                //ObstacleExclusiveTheme = _exclusiveTheme,
                //IsThemeExclusive = _isThemeExclusive
            };
        }
    }

}
