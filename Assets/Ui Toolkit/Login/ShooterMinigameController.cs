using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Core;

namespace ApexVelocity.UI
{
    public class ShooterMinigameController : MonoBehaviour
    {
        [Header("Game Balance Settings")]
        [SerializeField] private float gyroSensitivity = 1200f;
        [SerializeField] private float touchSensitivity = 1.2f;
        [SerializeField] private float laserSpeed = 800f;
        [SerializeField] private float enemySpeed = 240f;
        [SerializeField] private float spawnInterval = 0.7f;
        [SerializeField] private float autoFireRate = 0.18f;

        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;

        [SerializeField] private UIDocument _minigameDocument;
        private VisualElement _minigameRoot;
        private VisualElement _gameSpace;
        private VisualElement _playerShip;
        private Label _scoreLabel;
        private VisualElement _hpFill;
        private VisualElement _gameOverScreen;
        private Label _finalScoreLabel;

        private bool _isPlaying = false;
        private float _playerX = 300f;
        private float _playerY = 400f;
        private int _score = 0;
        private float _playerHp = 100f;
        private float _spawnTimer = 0f;
        private float _fireTimer = 0f;

        private readonly List<VisualElement> _lasers = new List<VisualElement>();
        private readonly List<VisualElement> _enemies = new List<VisualElement>();

        private void OnEnable()
        {
            var uiDocuments = GetComponents<UIDocument>();
            foreach (var doc in uiDocuments)
            {
                if (doc.visualTreeAsset != null && doc.visualTreeAsset.name.Contains("ShooterMinigame"))
                {
                    _minigameDocument = doc;
                    break;
                }
            }

            if (_minigameDocument == null && uiDocuments.Length > 1)
                _minigameDocument = uiDocuments[1];
            else if (_minigameDocument == null)
                _minigameDocument = GetComponent<UIDocument>();

            if (inputManager == null)
                inputManager = GetComponent<InputManager>();

            InitializeUI();
        }

        private void OnDisable()
        {
            UnbindInputEvents();
        }

        public void InitializeUI()
        {
            if (_minigameDocument == null || _minigameDocument.rootVisualElement == null) return;

            VisualElement root = _minigameDocument.rootVisualElement;

            _minigameRoot = root.Q<VisualElement>("minigame-root");
            _gameSpace = root.Q<VisualElement>("game-space");
            _playerShip = root.Q<VisualElement>("player-ship");
            _scoreLabel = root.Q<Label>("score-label");
            _hpFill = root.Q<Label>("hp-fill") ?? root.Q<VisualElement>("hp-fill");
            _gameOverScreen = root.Q<VisualElement>("game-over-screen");
            _finalScoreLabel = root.Q<Label>("final-score-label");

            var btnClose = root.Q<Button>("btn-close-minigame");
            if (btnClose != null) btnClose.clicked += HideMinigame;

            var btnRestart = root.Q<Button>("btn-restart");
            if (btnRestart != null) btnRestart.clicked += StartGame;

            var btnExit = root.Q<Button>("btn-exit-gameover");
            if (btnExit != null) btnExit.clicked += HideMinigame;

            if (_minigameRoot != null)
            {
                _minigameRoot.AddToClassList("mg-layer--hidden");
                _minigameRoot.style.display = DisplayStyle.None;
            }
        }

        public void ShowMinigame()
        {
            if (_minigameRoot == null) InitializeUI();

            if (_minigameRoot != null)
            {
                _minigameRoot.RemoveFromClassList("mg-layer--hidden");
                _minigameRoot.style.display = DisplayStyle.Flex;
            }

            BindInputEvents();
            StartGame();
        }

        public void HideMinigame()
        {
            _isPlaying = false;
            UnbindInputEvents();
            ClearEntities();

            if (_minigameRoot != null)
            {
                _minigameRoot.AddToClassList("mg-layer--hidden");
                _minigameRoot.style.display = DisplayStyle.None;
            }
        }

        private void BindInputEvents()
        {
            if (inputManager == null) return;

            inputManager.OnGyroMove += HandleGyroMove;
            inputManager.OnMoveTouchDelta += HandleTouchMove;
            inputManager.OnStartTouch += HandleStartTouch;
        }

        private void UnbindInputEvents()
        {
            if (inputManager == null) return;

            inputManager.OnGyroMove -= HandleGyroMove;
            inputManager.OnMoveTouchDelta -= HandleTouchMove;
            inputManager.OnStartTouch -= HandleStartTouch;
        }

        private void StartGame()
        {
            ClearEntities();
            _score = 0;
            _playerHp = 100f;
            _isPlaying = true;

            if (_gameOverScreen != null) _gameOverScreen.AddToClassList("mg-overlay--hidden");
            UpdateHud();

            if (inputManager != null && inputManager.IsGyroAvailable)
            {
                inputManager.CalibrateGyro();
            }

            float spaceWidth = _gameSpace != null ? _gameSpace.resolvedStyle.width : 600f;
            float spaceHeight = _gameSpace != null ? _gameSpace.resolvedStyle.height : 400f;

            _playerX = spaceWidth / 2f;
            _playerY = spaceHeight - 40f; // Pinned near the bottom edge

            UpdatePlayerPosition();
        }

        private void Update()
        {
            if (!_isPlaying || _gameSpace == null) return;

            // Auto-fire lasers continuously
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= autoFireRate)
            {
                _fireTimer = 0f;
                ShootLaser();
            }

            // Spawn Enemies / Asteroids
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnEnemy();
            }

            // Move Lasers upward
            // Update Lasers movement inside Update()
            for (int i = _lasers.Count - 1; i >= 0; i--)
            {
                var laser = _lasers[i];

                // Read style.top value or current computed float
                float currentY = laser.style.top.value.value;
                float y = currentY - (laserSpeed * Time.deltaTime);
                laser.style.top = y;

                if (y < -20f)
                {
                    _gameSpace.Remove(laser);
                    _lasers.RemoveAt(i);
                }
            }

            // Move Enemies downward & Handle Collisions
            float spaceHeight = _gameSpace.resolvedStyle.height;
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                float y = enemy.resolvedStyle.top + (enemySpeed * Time.deltaTime);
                enemy.style.top = y;

                // Collision: Laser vs Enemy
                for (int j = _lasers.Count - 1; j >= 0; j--)
                {
                    var laser = _lasers[j];
                    if (CheckCollision(laser, enemy))
                    {
                        _gameSpace.Remove(laser);
                        _lasers.RemoveAt(j);

                        _gameSpace.Remove(enemy);
                        _enemies.RemoveAt(i);

                        _score += 100;
                        UpdateHud();
                        break;
                    }
                }

                // Collision: Player Ship vs Enemy
                if (i < _enemies.Count && CheckCollision(_playerShip, enemy))
                {
                    _gameSpace.Remove(enemy);
                    _enemies.RemoveAt(i);

                    _playerHp -= 25f;
                    UpdateHud();

                    if (_playerHp <= 0f)
                    {
                        GameOver();
                        break;
                    }
                }

                // Despawn off-screen enemies
                if (i < _enemies.Count && y > spaceHeight + 40f)
                {
                    _gameSpace.Remove(enemy);
                    _enemies.RemoveAt(i);
                }
            }
        }

        #region Input Event Handlers

        private void HandleGyroMove(Vector2 tilt)
        {
            if (!_isPlaying || _gameSpace == null) return;

            // Tilt X moves ship left/right
            _playerX += tilt.x * gyroSensitivity * Time.deltaTime;

            // Tilt Y adjusts vertical offset slightly near the bottom
            float spaceHeight = _gameSpace.resolvedStyle.height;
            _playerY = Mathf.Clamp(_playerY - (tilt.y * gyroSensitivity * Time.deltaTime * 0.3f), spaceHeight - 120f, spaceHeight - 30f);

            UpdatePlayerPosition();
        }

        private void HandleTouchMove(Vector2 delta, float time, int touchIndex)
        {
            if (!_isPlaying || _gameSpace == null) return;

            // Drag finger to move ship horizontally and slightly vertically
            _playerX += delta.x * touchSensitivity;

            float spaceHeight = _gameSpace.resolvedStyle.height;
            _playerY = Mathf.Clamp(_playerY - (delta.y * touchSensitivity * 0.3f), spaceHeight - 120f, spaceHeight - 30f);

            UpdatePlayerPosition();
        }

        private void HandleStartTouch(Vector2 position, float time, int touchIndex)
        {
            if (!_isPlaying || _gameSpace == null) return;

            // Snap ship towards touch horizontal location
            _playerX = Mathf.Lerp(_playerX, position.x, 0.2f);
            UpdatePlayerPosition();
        }

        #endregion

        #region Gameplay Logic

        private void ShootLaser()
        {
            if (_gameSpace == null) return;

            var laser = new VisualElement();
            laser.AddToClassList("mg-laser");

            // Force absolute positioning so left and top inline styles take effect
            laser.style.position = Position.Absolute;

            // Explicit dimensions & fallback color (if not defined in USS)
            laser.style.width = 4f;
            laser.style.height = 16f;
            laser.style.backgroundColor = Color.red; // or Color.cyan

            laser.style.left = _playerX - 2f;
            laser.style.top = _playerY - 20f;

            _gameSpace.Add(laser);
            _lasers.Add(laser);

            Debug.Log("SHOOT LASER");
        }

        private void SpawnEnemy()
        {
            if (_gameSpace == null) return;

            var enemy = new VisualElement();
            enemy.AddToClassList(Random.value > 0.5f ? "mg-asteroid" : "mg-alien");

            float spaceWidth = _gameSpace.resolvedStyle.width;
            float spawnX = Random.Range(30f, Mathf.Max(30f, spaceWidth - 30f));

            enemy.style.left = spawnX;
            enemy.style.top = -30f;
            _gameSpace.Add(enemy);
            _enemies.Add(enemy);
        }

        private bool CheckCollision(VisualElement a, VisualElement b)
        {
            if (a == null || b == null) return false;

            float ax = a.resolvedStyle.left;
            float ay = a.resolvedStyle.top;
            float aw = a.resolvedStyle.width;
            float ah = a.resolvedStyle.height;

            float bx = b.resolvedStyle.left;
            float by = b.resolvedStyle.top;
            float bw = b.resolvedStyle.width;
            float bh = b.resolvedStyle.height;

            return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
        }

        private void UpdatePlayerPosition()
        {
            if (_playerShip == null || _gameSpace == null) return;

            float spaceWidth = _gameSpace.resolvedStyle.width;
            float spaceHeight = _gameSpace.resolvedStyle.height;

            float maxX = Mathf.Max(0f, spaceWidth - 32f);
            float maxY = Mathf.Max(0f, spaceHeight - 32f);

            _playerX = Mathf.Clamp(_playerX, 16f, spaceWidth - 16f);

            float clampedX = Mathf.Clamp(_playerX - 16f, 0f, maxX);
            float clampedY = Mathf.Clamp(_playerY - 16f, 0f, maxY);

            _playerShip.style.left = clampedX;
            _playerShip.style.top = clampedY;
        }

        private void UpdateHud()
        {
            if (_scoreLabel != null) _scoreLabel.text = $"SCORE  {_score:D4}";
            if (_hpFill != null) _hpFill.style.width = Length.Percent(Mathf.Max(0f, _playerHp));
        }

        private void GameOver()
        {
            _isPlaying = false;
            if (_gameOverScreen != null) _gameOverScreen.RemoveFromClassList("mg-overlay--hidden");
            if (_finalScoreLabel != null) _finalScoreLabel.text = $"FINAL SCORE: {_score}";
        }

        private void ClearEntities()
        {
            if (_gameSpace != null)
            {
                foreach (var laser in _lasers) _gameSpace.Remove(laser);
                foreach (var enemy in _enemies) _gameSpace.Remove(enemy);
            }
            _lasers.Clear();
            _enemies.Clear();
        }

        #endregion
    }
}