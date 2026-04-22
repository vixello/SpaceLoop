using Assets.Scripts.Core;
using Assets.Scripts.Data;
using Assets.Scripts.Data.Spaceship;
using Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Gameplay
{
    internal class SpaceCraftController : MonoBehaviour, IUpdateObserver
    {
        private InputManager _inputManager;
        private UpdatePublisher _publisher;
        private PlayerSpaceshipManager _spaceshipManager;

        [Header("Camera")]
        [SerializeField] private CinemachineCamera _camera;

        [Header("Movement")]
        [SerializeField] private float _maxSpeed = 50f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _turnSpeed = 60f;
        private float _currentSpeed;
        private float _forwardForceValue = 1f;

        [Header("Hover")]
        private float _hoverVelocity;
        [SerializeField] private float _hoverSpring = 120f;
        [SerializeField] private float _hoverDamper = 8f;
        [SerializeField] private float _breathingAmplitude = 0.1f;
        [SerializeField] private float _breathingFrequency = 1.2f;
        [SerializeField] private float _corePitchRoll = 2f;
        [SerializeField] private float _coreEngineRoll = 6f;

        [SerializeField] private LayerMask _groundLayer;
        [SerializeField]  private Rigidbody _rb;

        [Header("Hovera Points")]
        [SerializeField] private HoverPoint[] _hoverPoints;

        [Header("Visual Tilt (cosmetic roll)")]
        [SerializeField] private Transform _coreVisual;
        [SerializeField] private Transform _shipVisual;
        [SerializeField] private float _maxCoreRollAngle = 25f;
        [SerializeField] private float _maxEngineRollAngle = 4f;
        [SerializeField] private float _maxShipRollAngle = 14f;

        [SerializeField] private float _coreRollSmoothing = 8f;
        [SerializeField] private float _engineRollSmoothing = 15f;

        [Header("Core offset")]
        [SerializeField] private float _coreMaxOffsetX = 0.3f;
        [SerializeField] private float _coreOffsetSmoothing = 6f;

        private Vector3 _coreInitialLocalPos;

        [Header("PC Fallback")]
        [SerializeField] private float _keyboardSensitivity = 1f;


        private Vector2 _currentTilt;

        private float _coreRoll;
        private float _rollFromEngines = 0f;
        private float _pitch = 0f;

        private string _selectedShipID;
        [SerializeField] private SpaceshipConfig _shipConfig;

        [Inject]
        private void Construct(InputManager inputManager, UpdatePublisher updatePublisher, PlayerSpaceshipManager playerSpaceshipManager)
        {
            _inputManager = inputManager;
            _publisher = updatePublisher;
            _spaceshipManager = playerSpaceshipManager;
        }

        private async void Awake()
        {
            _inputManager.OnGyroMove += HandleGyroMove;
            _publisher.RegisterObserver(this);
            _coreInitialLocalPos = _coreVisual.localPosition;

            _selectedShipID = _spaceshipManager.GetSelectedSpaceship();

            // await _database.PopulateFromAddresablesLabel(); // has to be mvoed to spawner ?
            // await SetSelectedShipConfig(); // this too?

            _maxSpeed = _shipConfig.MaxSpeed;
            _acceleration = _shipConfig.Acceleration;
            _turnSpeed = _shipConfig.TurnSpeed;
            SetCameraTrackingTarget();
        }

/*        private async UniTask SetSelectedShipConfig()
        {
            _shipConfig = await _database.GetAsync(_selectedShipID);
        }
*/
        private void SetCameraTrackingTarget()
        {
            _camera.Target.TrackingTarget = _coreVisual;
        }

        private void OnDisable()
        {
            _inputManager.OnGyroMove -= HandleGyroMove;
            _publisher.UnregisterObserver(this);
        }

        private void FixedUpdate()
        {
            HandleHover();
            ApplyMovement();
            float thickness = 0.2f;

            Debug.DrawRay(transform.position , _lastGroundNormal * 3f, Color.red);
            Debug.DrawRay(transform.position - transform.right * thickness, transform.up * 3f, Color.green);
        }

        private Vector3 _lastGroundNormal = Vector3.up;
        private bool _wasGrounded;
        private bool _isGrounded;
        private void HandleHover()
        {
            float totalError = 0f;
            int validPoints = 0;

            float frontH = 0f, backH = 0f;
            int frontC = 0, backC = 0;
            float leftH = 0f, rightH = 0f;
            int leftC = 0, rightC = 0;
            Vector3 avgNormal = Vector3.zero;

            for (int i = 0; i < _hoverPoints.Length; i++)
            {
                var hp = _hoverPoints[i];

                Vector3 origin = hp.point.position;

                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, hp.hoverHeight * 3f, _groundLayer))
                {
                    float error = (hp.hoverHeight - hit.distance);

                    totalError += error * hp.forceMultiplier;
                    validPoints++;

                    {
                        avgNormal += hit.normal;
                        //normalCount++;
                    }
                    ApplyEngineHoverShift(hit, hp, ref leftH, ref leftC, ref rightH, ref rightC);
                    ComputeCoreSurfaceTilt(hp, hit, ref frontH, ref frontC, ref backH, ref backC);
                    ApplyCoreShift(hit, hp);

                    if (hp.point.localPosition.x > 0)
                    {
                        rightH += hit.distance;
                        rightC++;
                    }
                    else
                    {
                        leftH += hit.distance;
                        leftC++;
                    }
                }
            }
            float sum = rightH + leftH;
            float normalized = sum > 0 ? (rightH - leftH) / sum : 0f;

            _rollFromEngines = normalized * _coreEngineRoll;
            float frontAvg = frontC > 0 ? frontH / frontC : 0f;
            float backAvg = backC > 0 ? backH / backC : 0f;

            float pitchTarget = (backAvg - frontAvg) * 10f; // scale for feel

            _pitch = Mathf.Lerp(_pitch, pitchTarget, 6f * Time.fixedDeltaTime);

            _isGrounded = validPoints > 0;
            if (validPoints == 0)
            {
                _wasGrounded = true;
                _rb.AddForce(Vector3.up * Physics.gravity.y, ForceMode.Acceleration);

                // smooth return to upright in air
                Vector3 airNormal = Vector3.Lerp(_lastGroundNormal, Vector3.up, 2f * Time.fixedDeltaTime);
                _lastGroundNormal = airNormal;

                ApplyRotation(airNormal);
                return;
            }

            // ─────────────────────────────
            // 🟢 HOVER FORCE (SPRING)
            // ─────────────────────────────
            float avgError = totalError / validPoints;

            float breathing =
                Mathf.Sin(Time.time * _breathingFrequency)
                * _breathingAmplitude;

            float displacement = avgError + breathing;

            float springForce = displacement * _hoverSpring;
            float dampingForce = _rb.linearVelocity.y * _hoverDamper;

            float force = springForce - dampingForce;

            _rb.AddForce(transform.up * force, ForceMode.Acceleration);

            // clamp vertical velocity (stability)
            _rb.linearVelocity = new Vector3(
                _rb.linearVelocity.x,
                Mathf.Clamp(_rb.linearVelocity.y, -10f, 10f),
                _rb.linearVelocity.z
            );

            // ─────────────────────────────
            // 🟢 GROUND NORMAL
            // ─────────────────────────────
            avgNormal /= validPoints;
            Vector3 targetNormal = avgNormal.normalized;

            _lastGroundNormal = targetNormal;
            Debug.Log($"Last ground normal: {_lastGroundNormal.x} {_lastGroundNormal.y} {_lastGroundNormal.z}");

            ApplyRotation(_lastGroundNormal);
            _wasGrounded = false;
        }

        private void ApplyRotation(Vector3 groundNormal)
        {
            // 1. Choose a stable forward direction
            Vector3 forward = _rb.linearVelocity;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.01f)
                forward = transform.forward;

            forward.Normalize();

            // 2. Project forward onto the ground plane
            forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;

            // 3. Build rotation from corrected axes (safer than LookRotation alone)
            Quaternion targetRotation = Quaternion.LookRotation(forward, groundNormal);
            _rb.angularVelocity = Vector3.zero; 
            _rb.MoveRotation(targetRotation);
        }
        private void ApplyEngineHoverShift(RaycastHit hit, HoverPoint hp, ref float leftHeight, ref int leftCount, ref float rightHeight, ref int rightCount)
        {
            if (hp.visual == null) return;

            if (hp.Type != HoverPointType.Engine) return;
            // target height per engine
            float compression = hp.hoverHeight - hit.distance;
            float targetY = compression;
            Vector3 pos = hp.visual.localPosition;

            // smooth vertical bob
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);

            hp.visual.localPosition = pos;

            // engine banking response
            float roll = -_currentTilt.x * _maxEngineRollAngle;

            Quaternion targetRot = Quaternion.Euler(0, 0, roll);
            hp.visual.localRotation = Quaternion.Slerp(hp.visual.localRotation, targetRot, _engineRollSmoothing * Time.deltaTime);

            float h = hit.distance;
       
            if (hp.point.localPosition.x > 0)
            {
                rightHeight += h;
                rightCount++;
            }
            else
            {
                leftHeight += h;
                leftCount++;
            }
        }

        private void ApplyCoreShift(RaycastHit hit, HoverPoint hp)
        {
            if (_coreVisual == null) return;

            // target offset based on tilt

            float targetX = -_currentTilt.x * _coreMaxOffsetX;

            Vector3 pos = hp.visual.localPosition;
            if (hp.Type != HoverPointType.Engine)
            {
                float compression = hp.hoverHeight - hit.distance;
                float targetY = compression;
                pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            }

            Vector3 targetPos = _coreInitialLocalPos + new Vector3(targetX, pos.y, 0f);

            _coreVisual.localPosition = Vector3.Lerp(
                _coreVisual.localPosition,
                targetPos,
                _coreOffsetSmoothing * Time.deltaTime
            );
        }

        private void ComputeCoreSurfaceTilt(HoverPoint hp, RaycastHit hit, ref float frontHeight, ref int frontCount, ref float backHeight, ref int backCount)
        {
            if (hp.Type == HoverPointType.Engine) return;

            float h = hit.distance;

            if (hp.point.localPosition.z > 0)
            {
                frontHeight += h;
                frontCount++;
            }
            else
            {
                backHeight += h;
                backCount++;
            }
        }
        private void HandleGyroMove(Vector2 tilt)
        {
            _currentTilt = tilt;
        }

        public void ObservedUpdate()
        {
            if(!_inputManager.IsGyroAvailable) _currentTilt = ReadKeyboardFallback();
            //ApplyCosmedicRoll(_coreVisual, _maxCoreRollAngle, _coreRollSmoothing, _pitch, _rollFromEngines);
            //ApplyCosmedicRoll(_shipVisual, _maxEngineRollAngle, _engineRollSmoothing, 0f, 0f);
        }
        private void LateUpdate()
        {
            ApplyCosmedicRoll(_coreVisual, _maxCoreRollAngle, _coreRollSmoothing, _pitch, _rollFromEngines);
            ApplyCosmedicRoll(_shipVisual, _maxShipRollAngle, _engineRollSmoothing, 0f, 0f);
        }

        private void ApplyMovement()
        {
            Vector3 velocity = _rb.linearVelocity;

            // Input
            float inputX = _currentTilt.x; // left/right
            float inputZ = _forwardForceValue; // forward/back

            float targetSpeed = inputZ * _maxSpeed;

            // How was we are going forward and how fast we get to that speed (acceleration)
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _acceleration * Time.fixedDeltaTime);

            // ---  CURRENT FLAT VELOCITY  ---
            Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);

            // ---        STEERING         ---
            // Zamienia prędkość na zakres 0 - 1
            float speedFactor = Mathf.InverseLerp(0, _maxSpeed, Mathf.Abs(_currentSpeed));

            // Im szybciej jedizemy, tym gorzej skręcamy
            float turnStrength = Mathf.Lerp(_turnSpeed, _turnSpeed * 0.3f, speedFactor);

            if(flatVel.magnitude > 0.1f)
            {
                float turnAmount = inputX * turnStrength * Time.fixedDeltaTime;

                Quaternion turnRot = Quaternion.Euler(0f, turnAmount, 0f);
                flatVel = turnRot * flatVel;
            }

            Vector3 forwardVel = transform.forward * _currentSpeed;
            flatVel = Vector3.Lerp(flatVel, forwardVel, 2f * Time.fixedDeltaTime);

            // v = v + a * t;
            // Keep vertical velocity from hover
            velocity.x = flatVel.x;
            velocity.z = flatVel.z;

            _rb.linearVelocity = velocity;
            Debug.Log($"Veloctity x : {velocity.x}, y: {velocity.y} z: {velocity.z}");
/*            // --- ROTATE SHIP TOWARD MOVEMENT ---
            if (flatVel.magnitude > 0.5f)
            {
                transform.forward = Vector3.Slerp(
                    transform.forward,
                    flatVel.normalized,
                    6f * Time.fixedDeltaTime
                );
            }*/
        }

        /// <summary>Rolls the ship's visual mesh to give a sense of banking.</summary>
        private float _shipRoll;

        private void ApplyCosmedicRoll(
            Transform visual,
            float maxRollAngle,
            float smoothTime,
            float finalPitch,
            float rollFromEngines)
        {
            if (visual == null) return;

            float targetRoll = (-_currentTilt.x * maxRollAngle) + rollFromEngines;

            // choose correct stored state
            ref float rollState = ref (visual == _coreVisual ? ref _coreRoll : ref _shipRoll);

            // IMPORTANT: SmoothDamp on your OWN state, not Euler angles
            rollState = Mathf.Lerp(rollState, targetRoll, smoothTime * Time.deltaTime);

            visual.localRotation = Quaternion.Euler(finalPitch, 0f, rollState);
        }


        /*        private void ApplyCoreTiltFromHover(RaycastHit hit, HoverPoint hp, float error)
                {
                    Vector3 offset = hp.point.localPosition;

                    // FRONT/BACK → _pitch
                    _corePitch += offset.z * error;

                    // LEFT/RIGHT → roll
                    _coreRoll += offset.x * error;*//*
                }
        *//*
                private void ApplyCoreRotationFromHover()
                {
                    if (_coreVisual == null) return;

                    float _pitch = Mathf.Clamp(_corePitch * 5f, -10f, 10f);
                    float roll = Mathf.Clamp(-_coreRoll * 5f, -10f, 10f);

                    Quaternion targetRot = Quaternion.Euler(_pitch, 0f, roll);

                    _coreVisual.localRotation = Quaternion.Slerp(
                        _coreVisual.localRotation,
                        targetRot,
                        8f * Time.deltaTime
                    );
                }*/
        /// <summary>
        /// Call from a UI "Calibrate" button to re-zero the gyro to the
        /// player's current hand position.
        /// </summary>
        public void RequestGyroCalibration()
        {
            _inputManager?.CalibrateGyro();
        }

        private Vector2 ReadKeyboardFallback()
        {
            return _inputManager.GetMovementInput() * _keyboardSensitivity;
        }
    }
}
