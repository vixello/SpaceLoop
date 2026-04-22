using System;
using Assets.Scripts.Core.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        [Header("Obstacle Identity")]
        [SerializeField] protected ObstacleType _obstacleType = ObstacleType.Basic;
        [SerializeField] protected bool _isFirstBatch = false;

        [Header("Sync to Music")]
        [SerializeField] protected bool _isColllisiionSyncedToBeat = false;
        [SerializeField] protected bool _isSyncedToMelody = false;

        [Header("Theme Control")]
        //[SerializeField] protected bool _isThemeExclusive = false;
        //[SerializeField] protected ThemeType _exclusiveTheme = ThemeType.City;
        //public bool IsThemeExclusive => _isThemeExclusive;

        #region RENDERING
        protected SpriteRenderer _spriteRenderer;
        protected Sprite _currentSprite;
        #endregion

        #region POSITION AND ANIMATION
        protected float _obstacleSpeed;
        protected Vector3 _obstacleStartPosition = new Vector3(11.96f, 1.15f, 0f);
        protected Animator _obstacleAnimator;
        #endregion

        [Header("Behaviour Control")]
        private bool _hasUptadableBehaviour = false;
        private float _randomOffsetValueWaveBehaviour = 0f;
        protected ObstacleBehaviour _obstacleBehaviour = null;
        protected Action<GameObject, float> _onUpdateBehaviour = (go, f) => { }; //Do nothing, if the behaviour is not uptadable

        protected virtual void Start()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _currentSprite = (_spriteRenderer != null) ? _spriteRenderer.sprite : null;

            _obstacleAnimator = gameObject.GetComponent<Animator>();

            _obstacleSpeed = SpeedManager.Instance.GetSpeed(SpeedElement.obstacle);
            CreateObstacleBehavior(_obstacleType, _obstacleAnimator);
            gameObject.transform.position = _obstacleStartPosition;
        }

        private void OnEnable()
        {
            ApplyRandomOffsetIfApplicable();
            RunObstacleSpawnLogic();
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                _obstacleSpeed = SpeedManager.Instance.GetSpeed(SpeedElement.obstacle);
                MoveObstacle(_obstacleSpeed);
            }

            _onUpdateBehaviour(gameObject, _randomOffsetValueWaveBehaviour);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("ScreenEnd"))
            {
                Deactivate();
            }
            else if (collision.CompareTag("Player"))
            {
                OnPlayerCollisionBehavior();
            }
            else if (collision.CompareTag("Score"))
            {
                OnScoredBehaviour();
            }
        }

        protected void ChangePositionToReachObjectOnBeat()
        {
/*            if (_obstacleBehaviour is IOnBeatCollidable collidable)
            {
                collidable.CalculatePositionToCollideOnBeat(this.gameObject, _obstacleSpeed, ref _obstacleStartPosition);
                Debug.Log($"[DEBUG] New position to match beat: {_obstacleStartPosition}");
            }*/
        }

        private void MoveObstacle(float obstacleSpeed)
        {
            Vector3 currentPosition = gameObject.transform.position;
            Vector3 newPosition = currentPosition - new Vector3(obstacleSpeed * Time.deltaTime, 0, 0);

            gameObject.transform.position = newPosition;
        }

        private void CreateObstacleBehavior(ObstacleType obstacleType, Animator animator = null)
        {
            _obstacleBehaviour = ObstacleBehaviourFactory.Instance.CreateObstacleBehaviour(obstacleType, animator);
            _obstacleStartPosition = (Sprite != null) ? _obstacleBehaviour.SetupObstacleHeight(Sprite.bounds.size.x, transform.position.z) : _obstacleStartPosition;

/*            if (_obstacleBehaviour is IUpdatableObstacle updatable)
            {
                _onUpdateBehaviour = updatable.OnUpdateBehaviour;
                _hasUptadableBehaviour = true;
            }*/

            if (_isColllisiionSyncedToBeat)
            {
                ChangePositionToReachObjectOnBeat();
            }

            if (_isSyncedToMelody)
            {
                _obstacleBehaviour.OnMelodyBehaviour(gameObject, _obstacleSpeed);
            }

            _obstacleBehaviour.OnSpawnBehaviour(gameObject);
        }

        private void ApplyRandomOffsetIfApplicable()
        {
/*            if (_hasUptadableBehaviour)
            {
                _randomOffsetValueWaveBehaviour = Utils.Instance.GetRandomValue(0f, 4f);
            }*/
        }

        private void RunObstacleSpawnLogic()
        {
            if (_obstacleBehaviour == null)
                return;

            _obstacleBehaviour.OnSpawnBehaviour(gameObject);

            if (_isSyncedToMelody)
            {
                _obstacleBehaviour.OnMelodyBehaviour(gameObject, _obstacleSpeed);
            }
            if (_isColllisiionSyncedToBeat && _obstacleBehaviour != null)
            {
                ChangePositionToReachObjectOnBeat();
            }
            else if (_isColllisiionSyncedToBeat)
            {
                Debug.Log($"[DEBUG] New position cant be calcualted ");
                if (_obstacleBehaviour == null)
                    Debug.Log($"_obstacleBehaviour is null");
            }
        }

        private void OnPlayerCollisionBehavior()
        {
            _obstacleBehaviour?.OnPlayerCollisionBehavior();
        }

        private void OnScoredBehaviour()
        {
            _obstacleBehaviour?.OnScoredBehaviour();
        }

        public virtual void Deactivate()
        {
            Destroy(gameObject);
        }

        public Sprite Sprite { get { return _currentSprite; } }

        public void SetSprite(Sprite newSprite)
        {
            if (_spriteRenderer != null)
            {
                _currentSprite = newSprite;
                _spriteRenderer.sprite = _currentSprite;
            }
        }

    }

}
