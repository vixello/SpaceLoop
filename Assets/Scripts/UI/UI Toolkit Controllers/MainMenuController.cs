using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI 
{
    public class MainMenuController : UIController
    {
        // Buttons
        private Button _btnProfile;
        private Button _btnSettings;
        private Button _btnEvent;
        private Button _btnMissions;
        private Button _btnDriver;
        private Button _btnVehicle;
        private Button _btnRotateLeft;
        private Button _btnRotateRight;
        private Button _btnSolo;
        private Button _btnMultiplayer;
        private Button _btnGarage;
        private Button _btnShop;

        // Dropdown & Catcher Elements
        private VisualElement _settingsDropdown;
        private VisualElement _ddCatcher;
        private Button _btnDdSettings;
        private Button _btnDdSupport;
        private Button _btnDdFaq;
        private bool _isDropdownOpen = false;

        // Drag Zone for 3D Car Rotation
        private VisualElement _rotateDragZone;

        // Dynamic Labels & Fields
        private Label _playerLevel;
        private Label _playerName;
        private Label _eventTimer;
        private Label _eventTitle;
        private Label _missionsBadge;
        private Label _driverName;
        private Label _vehicleNameCard;
        private Label _vehicleNameStage;

        // Drag-to-rotate state variables
        private bool _isDragging;
        private Vector2 _previousPosition;
        [SerializeField] private float _rotationSpeed = 0.5f;

        public override void OnShow()
        {
            // Query Interactive Elements by Name from MainMenu.uxml
            _btnProfile = _root.Q<Button>("btn-profile");
            _btnSettings = _root.Q<Button>("btn-settings");
            _btnEvent = _root.Q<Button>("btn-event");
            _btnMissions = _root.Q<Button>("btn-missions");
            _btnDriver = _root.Q<Button>("btn-driver");
            _btnVehicle = _root.Q<Button>("btn-vehicle");
            _btnRotateLeft = _root.Q<Button>("rotate-left");
            _btnRotateRight = _root.Q<Button>("rotate-right");
            _btnSolo = _root.Q<Button>("btn-solo");
            _btnMultiplayer = _root.Q<Button>("btn-multiplayer");
            _btnGarage = _root.Q<Button>("btn-garage");
            _btnShop = _root.Q<Button>("btn-shop");
            _rotateDragZone = _root.Q<VisualElement>("rotate-drag-zone");

            // Query Dropdown Elements[cite: 1]
            _settingsDropdown = _root.Q<VisualElement>("settings-dropdown");
            _ddCatcher = _root.Q<VisualElement>("dd-catcher");
            _btnDdSettings = _root.Q<Button>("btn-dd-settings");
            _btnDdSupport = _root.Q<Button>("btn-dd-support");
            _btnDdFaq = _root.Q<Button>("btn-dd-faq");

            // Query Labels for dynamic data population
            _playerLevel = _root.Q<Label>("player-level");
            _playerName = _root.Q<Label>("player-name");
            _eventTimer = _root.Q<Label>("event-timer");
            _eventTitle = _root.Q<Label>("event-title");
            _missionsBadge = _root.Q<Label>("missions-badge");
            _driverName = _root.Q<Label>("driver-name");
            _vehicleNameCard = _root.Q<Label>("vehicle-name-card");
            _vehicleNameStage = _root.Q<Label>("vehicle-name");

            RegisterCallbacks();
            LoadPlayerData(); // Populate initial text values if needed
        }

        public override void OnHide()
        {
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_btnProfile != null) _btnProfile.clicked += OnProfileClicked;
            if (_btnSettings != null) _btnSettings.clicked += OnSettingsClicked;
            if (_btnEvent != null) _btnEvent.clicked += OnEventClicked;
            if (_btnMissions != null) _btnMissions.clicked += OnMissionsClicked;
            if (_btnDriver != null) _btnDriver.clicked += OnDriverClicked;
            if (_btnVehicle != null) _btnVehicle.clicked += OnVehicleClicked;
            if (_btnRotateLeft != null) _btnRotateLeft.clicked += OnRotateLeftClicked;
            if (_btnRotateRight != null) _btnRotateRight.clicked += OnRotateRightClicked;
            if (_btnSolo != null) _btnSolo.clicked += OnSoloClicked;
            if (_btnMultiplayer != null) _btnMultiplayer.clicked += OnMultiplayerClicked;
            if (_btnGarage != null) _btnGarage.clicked += OnGarageClicked;
            if (_btnShop != null) _btnShop.clicked += OnShopClicked;

            // Dropdown Option Callbacks[cite: 1]
            if (_btnDdSettings != null) _btnDdSettings.clicked += OnDdSettingsClicked;
            if (_btnDdSupport != null) _btnDdSupport.clicked += OnDdSupportClicked;
            if (_btnDdFaq != null) _btnDdFaq.clicked += OnDdFaqClicked;

            // Catcher to close dropdown on background click
            if (_ddCatcher != null)
            {
                _ddCatcher.RegisterCallback<PointerDownEvent>(OnCatcherPointerDown);
            }

            // 3D Scene Drag-to-Rotate Callbacks
            if (_rotateDragZone != null)
            {
                _rotateDragZone.RegisterCallback<PointerDownEvent>(OnPointerDown);
                _rotateDragZone.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                _rotateDragZone.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }
        }

        private void UnregisterCallbacks()
        {
            if (_btnProfile != null) _btnProfile.clicked -= OnProfileClicked;
            if (_btnSettings != null) _btnSettings.clicked -= OnSettingsClicked;
            if (_btnEvent != null) _btnEvent.clicked -= OnEventClicked;
            if (_btnMissions != null) _btnMissions.clicked -= OnMissionsClicked;
            if (_btnDriver != null) _btnDriver.clicked -= OnDriverClicked;
            if (_btnVehicle != null) _btnVehicle.clicked -= OnVehicleClicked;
            if (_btnRotateLeft != null) _btnRotateLeft.clicked -= OnRotateLeftClicked;
            if (_btnRotateRight != null) _btnRotateRight.clicked -= OnRotateRightClicked;
            if (_btnSolo != null) _btnSolo.clicked -= OnSoloClicked;
            if (_btnMultiplayer != null) _btnMultiplayer.clicked -= OnMultiplayerClicked;
            if (_btnGarage != null) _btnGarage.clicked -= OnGarageClicked;
            if (_btnShop != null) _btnShop.clicked -= OnShopClicked;

            if (_btnDdSettings != null) _btnDdSettings.clicked -= OnDdSettingsClicked;
            if (_btnDdSupport != null) _btnDdSupport.clicked -= OnDdSupportClicked;
            if (_btnDdFaq != null) _btnDdFaq.clicked -= OnDdFaqClicked;

            if (_ddCatcher != null)
            {
                _ddCatcher.UnregisterCallback<PointerDownEvent>(OnCatcherPointerDown);

            }
            if (_rotateDragZone != null)
            {
                _rotateDragZone.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                _rotateDragZone.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                _rotateDragZone.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            }
        }

        private void LoadPlayerData()
        {
            // Example: populate fields dynamically or hook up to a SaveManager / PlayerProfile script
            if (_playerName != null) _playerName.text = "NOVA_RUNNER";
            if (_playerLevel != null) _playerLevel.text = "27";
            if (_driverName != null) _driverName.text = "KAI NOVA";
            if (_vehicleNameCard != null) _vehicleNameCard.text = "ZEPHYR GT-X";
            if (_vehicleNameStage != null) _vehicleNameStage.text = "ZEPHYR GT-X";
        }

        // --- Navigation & Action Callbacks ---

        private void OnProfileClicked()
        {
            Debug.Log("Opening User Profile / Edit Profile screen.");
            // TODO: Load Profile UI panel or scene
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Opening Settings overlay.");
            ToggleDropdown();
        }

        private void OnEventClicked()
        {
            Debug.Log("Opening Live Event screen.");
        }

        private void OnMissionsClicked()
        {
            Debug.Log("Opening Missions / Challenges screen.");
        }

        private void OnDriverClicked()
        {
            Debug.Log("Opening Driver Customization / Selection.");
        }

        private void OnVehicleClicked()
        {
            Debug.Log("Opening Vehicle Customization / Garage.");
        }

        private void OnRotateLeftClicked()
        {
            Debug.Log("Stepping 3D Showcase Vehicle Left.");
            // TODO: Rotate 3D vehicle transform left by a fixed angle
        }

        private void OnRotateRightClicked()
        {
            Debug.Log("Stepping 3D Showcase Vehicle Right.");
            // TODO: Rotate 3D vehicle transform right by a fixed angle
        }

        private void OnSoloClicked()
        {
            Debug.Log("Launching Solo Racing Campaign / Story screen.");
            // TODO: Load Story scene or activate Story UI document
        }

        private void OnMultiplayerClicked()
        {
            Debug.Log("Launching Multiplayer Lobby / Staging Grid.");
            // TODO: Load Multiplayer / Matchmaking screen
        }

        private void OnGarageClicked()
        {
            Debug.Log("Opening Garage screen.");
            // TODO: Load Garage scene/panel
        }

        private void OnShopClicked()
        {
            Debug.Log("Opening In-Game Shop.");
            // TODO: Load Shop panel
        }
        private void ToggleDropdown()
        {
            if (_isDropdownOpen) CloseDropdown();
            else OpenDropdown();
        }
        private void OpenDropdown()
        {
            _isDropdownOpen = true;
            if (_settingsDropdown != null) _settingsDropdown.RemoveFromClassList("dd--hidden");
            if (_ddCatcher != null) _ddCatcher.RemoveFromClassList("dd--hidden");
        }

        private void CloseDropdown()
        {
            _isDropdownOpen = false;
            if (_settingsDropdown != null) _settingsDropdown.AddToClassList("dd--hidden");
            if (_ddCatcher != null) _ddCatcher.AddToClassList("dd--hidden");
        }

        private void OnCatcherPointerDown(PointerDownEvent evt)
        {
            CloseDropdown();
        }

        private void OnDdSettingsClicked()
        {
            Debug.Log("Dropdown: Opening Settings popup.");
            CloseDropdown();
        }

        private void OnDdSupportClicked()
        {
            Debug.Log("Dropdown: Opening Support.");
            CloseDropdown();
        }

        private void OnDdFaqClicked()
        {
            Debug.Log("Dropdown: Opening FAQ.");
            CloseDropdown();
        }

        // --- 3D Scene Drag-to-Rotate Implementation ---

        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDragging = true;
            _previousPosition = evt.position;
            _rotateDragZone.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;

            float deltaX = evt.position.x - _previousPosition.x;
            _previousPosition = evt.position;

            // Rotate the 3D car GameObject in your scene
            // Example: Camera.main or a designated CarTransform reference:
            // if (showcaseCarTransform != null)
            //     showcaseCarTransform.Rotate(Vector3.up, -deltaX * _rotationSpeed, Space.World);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging) return;

            _isDragging = false;
            _rotateDragZone.ReleasePointer(evt.pointerId);
        }
    }
}
