using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Assets.Scripts.Core
{
    [DefaultExecutionOrder(-4)]
    public class InputManager : MonoBehaviour
    {
        private InputSystem_Actions _playerInput;

        // ── Touch events (unchanged) ──────────────────────────────────────────
        public delegate void StartTouch(Vector2 position, float time, int touchIndex);
        public event StartTouch OnStartTouch;
        public delegate void EndTouch(Vector2 position, float time, int touchIndex);
        public event EndTouch OnEndTouch;
        public delegate void MoveTouch(Vector2 position, float time, int touchIndex);
        public event MoveTouch OnMoveTouchDelta;
        public event MoveTouch OnMoveTouchScreenPosition;
        public delegate void HoldTouch(Vector2 position, float time, int touchIndex);
        public event HoldTouch OnHoldTouch;
        public delegate void DoubleTap();
        public event DoubleTap OnDoubleTap;
        public delegate void LeftJoystickMove(Vector2 position);
        public event LeftJoystickMove OnLeftJoystickMove;

        // ── Gyro event ──
        /// <summary>
        /// Fires every frame when gyro is active.
        /// X = horizontal tilt (-1 left … +1 right)
        /// Y = vertical tilt   (-1 down  … +1 up)
        /// Values are smoothed and calibrated.
        /// </summary>
        public delegate void GyroMove(Vector2 tilt);
        public event GyroMove OnGyroMove;

        // ── Gyro settings (tune in Inspector) ───
        [Header("Gyro Settings")]
        [SerializeField] private float _gyroSensitivity = 6f;
        /// <summary>How fast the smoothed value follows the raw value (0 = no lag, 1 = frozen).</summary>
        [SerializeField][Range(0f, 1f)] private float _gyroSmoothing = 0.05f;
        [SerializeField] private float _gyroDeadZone = 0.02f;


        // ── Gyro runtime state ──
        private bool _gyroAvailable = false;
        private Vector2 _gyroCalibrationOffset = Vector2.zero;   // set on Calibrate()
        private Vector2 _gyroSmoothedTilt = Vector2.zero;
        private UnityEngine.InputSystem.AttitudeSensor _attitude;
        private Quaternion _calibrationRotation;


        // ── Touch runtime state (unchanged) ──
        private bool _isTouchActive = false;
        private float _lastTapTime = 0f;
        private const float _doubleTapThreshold = 0.3f;
        private Vector2 _lastTapPosition;
        private readonly float _excludedBottomHeight = 400f;
        private readonly Dictionary<int, Coroutine> _trackers = new();
        private Dictionary<int, Vector2> _lastPositions = new();
        [SerializeField] private float touchDeadZone = 0.5f;


        // ─────────────────────────────────────────────────────────────────────
        #region Unity lifecycle

        private void Awake()
        {
            Application.targetFrameRate = 60;
            _playerInput = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _playerInput.Enable();
            EnhancedTouchSupport.Enable();

            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += FingerUp;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += FingerMvoe;

            _playerInput.Player.Move.performed += MoveLeftJoystick;
            _playerInput.Player.Move.canceled += MoveLeftJoystick;

            InitGyro();
        }

        private void OnDisable()
        {
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= FingerUp;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= FingerMvoe;

            _playerInput.Player.Move.performed -= MoveLeftJoystick;
            _playerInput.Player.Move.canceled -= MoveLeftJoystick;
        }

        private void Update()
        {
            // Legacy mouse scroll — kept from original
            var scroll = Mouse.current?.scroll.ReadValue();

            if (_gyroAvailable)
                UpdateGyro();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Gyro
        private void InitGyro()
        {
            _attitude = UnityEngine.InputSystem.AttitudeSensor.current;

            if (_attitude != null)
            {
                InputSystem.EnableDevice(_attitude);
                _gyroAvailable = true;

                CalibrateGyro();

                Debug.Log("[InputManager] GravitySensor initialised (portrait mode).");
            }
            else
            {
                _gyroAvailable = false;
                Debug.LogWarning("[InputManager] No gravity sensor found — gyro disabled.");
            }
        }

        /// <summary>
        /// Call this any time you want to re-zero the gyro to the current
        /// physical orientation (e.g. when the player taps "Ready").
        /// </summary>
        public void CalibrateGyro()
        {
            _calibrationRotation = _attitude.attitude.ReadValue();
        }

        private void UpdateGyro()
        {
            Quaternion currentAttitude = _attitude.attitude.ReadValue();

            // Get relative rotation from calibration
            Quaternion delta = currentAttitude * Quaternion.Inverse(_calibrationRotation);

            Vector3 up = delta * Vector3.up;

            // Horizontal tilt = how much "up" leans sideways
            float rawX = up.x;

            // Vertical tilt = forward/back tilt
            float rawY = up.z;

            Vector2 rawTilt = new Vector2(rawX, rawY) * _gyroSensitivity;

            // Dead zone
            if (Mathf.Abs(rawTilt.x) < _gyroDeadZone) rawTilt.x = 0f;
            if (Mathf.Abs(rawTilt.y) < _gyroDeadZone) rawTilt.y = 0f;

            rawTilt = Vector2.ClampMagnitude(rawTilt, 1f);

            _gyroSmoothedTilt = Vector2.Lerp(
                _gyroSmoothedTilt,
                rawTilt,
                1f - _gyroSmoothing
            );

            OnGyroMove?.Invoke(_gyroSmoothedTilt);
        }

        /// <summary>Converts 0-360 Euler angles to -180..180.</summary>
        private static float NormaliseAngle(float angle)
            => angle > 180f ? angle - 360f : angle;

        /// <summary>
        /// Returns the latest smoothed gyro tilt without an event subscription.
        /// X = horizontal (-1…+1), Y = vertical (-1…+1).
        /// Returns Vector2.zero if no gyro is available.
        /// </summary>
        public Vector2 GetGyroTilt() => _gyroAvailable ? _gyroSmoothedTilt : Vector2.zero;

        /// <summary>True if a physical gyroscope was found on this device.</summary>
        public bool IsGyroAvailable => _gyroAvailable;

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Touch (unchanged from original)

        private void MoveLeftJoystick(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            OnLeftJoystickMove?.Invoke(input);
        }

        private void FingerDown(Finger finger)
        {
            int id = finger.index;
            float currentTime = Time.time;
            Vector2 currentTapPosition = finger.screenPosition;

            if (IsInExcludedArea(currentTapPosition)) return;

            _lastPositions[finger.index] = finger.screenPosition;

            OnStartTouch?.Invoke(finger.screenPosition, Time.time, id);

            if (currentTime - _lastTapTime < _doubleTapThreshold &&
                Vector2.Distance(_lastTapPosition, currentTapPosition) < 50f)
            {
                OnDoubleTap?.Invoke();
            }

            _isTouchActive = true;
            Coroutine c = StartCoroutine(TrackFingerCoroutine(finger));
            _trackers[id] = c;

            _lastTapTime = currentTime;
            _lastTapPosition = currentTapPosition;
        }

        private void FingerUp(Finger finger)
        {
            Vector2 currentTapPosition = finger.screenPosition;

            if (_trackers.TryGetValue(finger.index, out var c))
            {
                StopCoroutine(c);
                _trackers.Remove(finger.index);
            }

            if (IsInExcludedArea(currentTapPosition)) return;

            _lastPositions.Remove(finger.index);
            OnEndTouch?.Invoke(finger.screenPosition, Time.time, finger.index);

            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeFingers.Count == 0)
                _isTouchActive = false;
        }

        private void FingerMvoe(Finger finger)
        {
            if (IsInExcludedArea(finger.screenPosition)) return;
            if (!_lastPositions.TryGetValue(finger.index, out var lastPos)) return;

            Vector2 currentPos = finger.screenPosition;
            Vector2 delta = currentPos - lastPos;
            _lastPositions[finger.index] = currentPos;

            if (delta.sqrMagnitude < touchDeadZone * touchDeadZone) return;

            delta.x = Mathf.Clamp(delta.x, -180, 180);
            OnMoveTouchDelta?.Invoke(delta, Time.time, finger.index);
            OnMoveTouchScreenPosition?.Invoke(finger.screenPosition, Time.time, finger.index);
        }

        private bool IsInExcludedArea(Vector2 position)
            => position.y < _excludedBottomHeight &&
               (position.x > Screen.width - 440f || position.x < 450f);

        public float GetHorizontalMovementInput()
            => _playerInput.Player.Move.ReadValue<Vector2>().x;

        public bool IsJumpTriggered()
            => _playerInput.Player.Jump.triggered;

        private IEnumerator TrackFingerCoroutine(Finger finger)
        {
            while (_isTouchActive)
            {
                if (finger == null) yield break;
                OnHoldTouch?.Invoke(finger.screenPosition, Time.time, finger.index);
                yield return null;
            }
        }

        #endregion


        public Vector2 GetMovementInput()
        {
            return _playerInput.Player.Move.ReadValue<Vector2>();
        }
    }
}
