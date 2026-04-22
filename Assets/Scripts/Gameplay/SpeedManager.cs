using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public enum SpeedElement
    {
        obstacle = 0,
        background_0 = 1,
        background_1 = 2,
        background_2 = 3,
        background_3 = 4,
        background_4 = 5,
        background_5 = 6,
        laneNote = 7
    }


    [DefaultExecutionOrder(-1)]
    public class SpeedManager
    {
        private float _elapsedTime = 0f;
        private float _initialSpeed;
        private float _targetSpeed;
        private float _duration;
        private Action<float> _onSpeedChanged;

        private bool _isIncreasingSpeed = false;
        private bool _isDecreasingSpeed = false;
        private float _speedDuringIncrease;
        private float _currentSpeed;

        private bool isMovementStopped = false;

        #region Speed values
        protected float _obstacleSpeed = 5f;
        protected float _noteLaneSpeed = 3f;
        protected float[] _backgroundSpeeds = new float[6];
        #endregion

        float _speedIncrement = 1f;

        private static SpeedManager _instance;
        public static SpeedManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SpeedManager();
                return _instance;
            }
        }

        public SpeedManager()
        {
            //EventBus.Instance.OnIncreaseElementSpeed += SmoothIncreaseSpeed;
            //EventBus.Instance.OnStopElementMovement += ToggleMovementStop;
        }

        ~SpeedManager()
        {
            //EventBus.Instance.OnIncreaseElementSpeed -= SmoothIncreaseSpeed;
            //EventBus.Instance.OnStopElementMovement -= ToggleMovementStop;
        }

        public void StartSpeedIncrease(float initialSpeed, float targetSpeed, float duration)
        {
            if (_isIncreasingSpeed || _isDecreasingSpeed)
            {
                Debug.Log("⚠️ Speed increase ignored: Already in progress!");
                return;
            }

            _initialSpeed = initialSpeed;
            _targetSpeed = targetSpeed;
            _duration = duration;

            // Reset time and flags
            _elapsedTime = 0f;
            _isIncreasingSpeed = true;
            _currentSpeed = _initialSpeed;
            SetSpeed(initialSpeed, SpeedElement.obstacle);

            _onSpeedChanged?.Invoke(_initialSpeed);
        }

        public void StartSpeedDecrease(float targetSpeed, float duration)
        {
            _targetSpeed = targetSpeed;
            _duration = duration;
            _elapsedTime = 0f;
            _isDecreasingSpeed = true;
        }

        public void UpdateSpeed(float deltaTime)
        {
            if (_isIncreasingSpeed)
            {
                // Increase speed logic (similar to the coroutine)
                _elapsedTime += deltaTime;
                _currentSpeed = Mathf.Lerp(_initialSpeed, _targetSpeed, _elapsedTime / _duration);

                _onSpeedChanged?.Invoke(_currentSpeed);

                if (_elapsedTime >= _duration)
                {
                    // Once the speed increase is finished, start decreasing it
                    _isIncreasingSpeed = false;
                    _isDecreasingSpeed = true;
                    _elapsedTime = 0f;
                    _speedDuringIncrease = _currentSpeed;
                }
                SetSpeed(_currentSpeed, SpeedElement.obstacle);

            }

            else if (_isDecreasingSpeed)
            {
                // Decrease speed logic (similar to the coroutine)
                _elapsedTime += deltaTime;
                _currentSpeed = Mathf.Lerp(_speedDuringIncrease, _initialSpeed, _elapsedTime / (_duration / 5));

                _onSpeedChanged?.Invoke(_currentSpeed);

                if (_elapsedTime >= (_duration / 5))
                {
                    // Once the speed decrease is finished, reset the process
                    _isDecreasingSpeed = false;
                    _currentSpeed = _initialSpeed;
                    _elapsedTime = 0f;
                }
                SetSpeed(_currentSpeed, SpeedElement.obstacle);
            }
        }

        private void SmoothIncreaseSpeed(float duration)
        {
            float targetSpeed = _speedIncrement;

            StartSpeedIncrease(_obstacleSpeed, targetSpeed, duration);
        }

        internal float GetCurrentSpeed()
        {
            return _currentSpeed;
        }

        internal void SetSpeed(float newSpeed, SpeedElement speedElement)
        {
            switch (speedElement)
            {
                case SpeedElement.obstacle:
                    _obstacleSpeed = newSpeed;
                    break;

                default:
                    Debug.LogError("Speed element doesn't exist");
                    break;
            }
        }

        public float GetSpeed(SpeedElement speedElement)
        {
            if (!isMovementStopped)
            {
                switch (speedElement)
                {
                    case SpeedElement.obstacle:
                        //if (GameStateManager.Instance.GameState == GameState.Loading) return 0f;
                        return _obstacleSpeed;

                    default:
                        return 0f;
                }
            }
            else
                switch (speedElement)
                {
                    case SpeedElement.laneNote:
                        return _noteLaneSpeed;
                    default:
                        return 0f;
                }
        }

        private void ToggleMovementStop(bool isStopped)
        {
            isMovementStopped = isStopped;
        }
    }

}
