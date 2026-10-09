using System;
using System.Linq; // Wymagane dla ToArray() na kolekcjach UI Toolkit
using UnityEngine;
using UnityEngine.UIElements;

namespace ApexVelocity.UI
{
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ShooterMinigameController))]
    public class LoginController : MonoBehaviour
    {
        [Header("Tachometer Animation Settings")]
        [SerializeField] private float minNeedleAngle = -52f;
        [SerializeField] private float maxNeedleAngle = 100f;
        [SerializeField] private float tachAnimationSpeed = 5f;

        // UI Document & Root Elements
        private UIDocument _document;
        private VisualElement _root;

        // Pages
        private VisualElement _pageLogin;
        private VisualElement _pageReturning;

        // Buttons
        private Button _btnGoogleSignIn;
        private Button _btnAuthGuest;
        private Button _btnAuthEmail;
        private Button _btnAuthMore;
        private Button _btnContinue;
        private Button _btnOtherAccount;
        private Button _btnSignOut;
        private Button _btnMinigame;

        // HUD & Tachometer
        private VisualElement _tachNeedle;
        private Label _tachSpeedLabel;
        private Label _downloadPctLabel;
        private VisualElement _downloadFill;

        // Overlays
        private VisualElement _overlaySigning;
        private VisualElement _overlayError;
        private Button _btnSigninCancel;
        private Button _btnErrorClose;
        private Button _btnErrorRetry;
        private Label _errorTextLabel;
        private Label _errorCodeLabel;

        // Animation State Variables
        private float _targetNeedleAngle = -52f;
        private float _currentNeedleAngle = -52f;
        private bool _isAuthenticating = false;

        // Events
        public event Action OnMinigameRequested;
        public event Action<string> OnGoogleSignInRequested;
        public event Action OnGuestSignInRequested;
        public event Action OnContinueRequested;

        private ShooterMinigameController _minigameController;
        
        private void OnEnable()
        {
            _document = GetComponent<UIDocument>();
            if (_document == null) return;

            _root = _document.rootVisualElement;
            if (_root == null) return;
        
            _minigameController = GetComponent<ShooterMinigameController>();

            QueryElements();
            RegisterCallbacks();

            if (_minigameController != null)
            {
                OnMinigameRequested += _minigameController.ShowMinigame;
            }
            SetInitialState();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
            if (_minigameController != null)
            {
                OnMinigameRequested -= _minigameController.ShowMinigame;
            }        
        }

        private void Update()
        {
            if (Mathf.Abs(_currentNeedleAngle - _targetNeedleAngle) > 0.1f)
            {
                _currentNeedleAngle = Mathf.Lerp(_currentNeedleAngle, _targetNeedleAngle, Time.deltaTime * tachAnimationSpeed);
                
                if (_tachNeedle != null)
                {
                    _tachNeedle.style.rotate = new Rotate(new Angle(_currentNeedleAngle, AngleUnit.Degree));
                }

                if (_tachSpeedLabel != null)
                {
                    float normalized = Mathf.InverseLerp(minNeedleAngle, maxNeedleAngle, _currentNeedleAngle);
                    int kmh = Mathf.RoundToInt(normalized * 340f);
                    _tachSpeedLabel.text = kmh.ToString("D3");
                }
            }
        }

        #region Initialization & Setup

        private void QueryElements()
        {
            // Pages
            _pageLogin = _root.Q<VisualElement>("page-login");
            _pageReturning = _root.Q<VisualElement>("page-returning");

            // Login Buttons
            _btnGoogleSignIn = _root.Q<Button>("btn-google-signin");
            _btnAuthGuest = _root.Q<Button>("btn-auth-guest");
            _btnAuthEmail = _root.Q<Button>("btn-auth-email");
            _btnAuthMore = _root.Q<Button>("btn-auth-more");

            // Returning Player Controls
            _btnContinue = _root.Q<Button>("btn-continue");
            _btnOtherAccount = _root.Q<Button>("btn-other-account");
            _btnSignOut = _root.Q<Button>("btn-sign-out");

            // Boot & Minigame
            _btnMinigame = _root.Q<Button>("btn-minigame");
            _downloadPctLabel = _root.Q<Label>(className: "boot__pct");
            _downloadFill = _root.Q<VisualElement>(className: "boot__fill");

            // Tachometer Elements
            _tachNeedle = _root.Q<VisualElement>("tach-needle");
            _tachSpeedLabel = _root.Q<Label>("tach-speed");

            // Overlays
            _overlaySigning = _root.Q<VisualElement>("overlay-signing");
            _overlayError = _root.Q<VisualElement>("overlay-error");
            _btnSigninCancel = _root.Q<Button>("btn-signin-cancel");
            _btnErrorClose = _root.Q<Button>("btn-error-close");
            _btnErrorRetry = _root.Q<Button>("btn-error-retry");
            _errorTextLabel = _root.Q<Label>("error-text");
            _errorCodeLabel = _root.Q<Label>("error-code");
        }

        private void RegisterCallbacks()
        {
            // Authentication Handlers
            if (_btnGoogleSignIn != null) _btnGoogleSignIn.clicked += HandleGoogleSignIn;
            if (_btnAuthGuest != null) _btnAuthGuest.clicked += HandleGuestSignIn;
            if (_btnAuthEmail != null) _btnAuthEmail.clicked += () => Debug.Log("[Login] Email auth clicked.");
            if (_btnAuthMore != null) _btnAuthMore.clicked += () => Debug.Log("[Login] More auth providers clicked.");

            // Returning Player Handlers
            if (_btnContinue != null) _btnContinue.clicked += HandleContinueReturningPlayer;
            if (_btnOtherAccount != null) _btnOtherAccount.clicked += ShowLoginPage;
            if (_btnSignOut != null) _btnSignOut.clicked += ShowLoginPage;

            // Minigame Trigger
            if (_btnMinigame != null) _btnMinigame.clicked += () => OnMinigameRequested?.Invoke();

            // Overlay Handlers
            if (_btnSigninCancel != null) _btnSigninCancel.clicked += CancelAuthentication;
            if (_btnErrorClose != null) _btnErrorClose.clicked += HideErrorOverlay;
            if (_btnErrorRetry != null) _btnErrorRetry.clicked += HandleGoogleSignIn;
        }

        private void UnregisterCallbacks()
        {
            if (_btnGoogleSignIn != null) _btnGoogleSignIn.clicked -= HandleGoogleSignIn;
            if (_btnAuthGuest != null) _btnAuthGuest.clicked -= HandleGuestSignIn;
            if (_btnContinue != null) _btnContinue.clicked -= HandleContinueReturningPlayer;
            if (_btnOtherAccount != null) _btnOtherAccount.clicked -= ShowLoginPage;
            if (_btnSignOut != null) _btnSignOut.clicked -= ShowLoginPage;
            if (_btnSigninCancel != null) _btnSigninCancel.clicked -= CancelAuthentication;
            if (_btnErrorClose != null) _btnErrorClose.clicked -= HideErrorOverlay;
        }

        private void SetInitialState()
        {
            ShowLoginPage();
            HideOverlays();
            SetNeedleAngle(minNeedleAngle);
        }

        #endregion

        #region Page Navigation

        public void ShowLoginPage()
        {
            SetPageVisible(_pageLogin, true);
            SetPageVisible(_pageReturning, false);
            SetNeedleAngle(minNeedleAngle);
        }

        public void ShowReturningPlayerPage(string playerName, string levelText, string subInfo)
        {
            SetPageVisible(_pageLogin, false);
            SetPageVisible(_pageReturning, true);

            var nameLabel = _root.Q<Label>(className: "ret__name");
            if (nameLabel != null) nameLabel.text = playerName;

            var lvlLabel = _root.Q<Label>(className: "ret__lvl-text");
            if (lvlLabel != null) lvlLabel.text = levelText;

            var subLabel = _root.Q<Label>(className: "ret__sub");
            if (subLabel != null) subLabel.text = subInfo;

            SetNeedleAngle(0f);
        }

        private void SetPageVisible(VisualElement page, bool visible)
        {
            if (page == null) return;
            var parent = page.parent;
            if (parent != null)
            {
                if (visible) parent.RemoveFromClassList("page--hidden");
                else parent.AddToClassList("page--hidden");
            }
        }

        #endregion

        #region Authentication Actions

        private void HandleGoogleSignIn()
        {
            StartAuthenticating();
            OnGoogleSignInRequested?.Invoke("google_id_token");
        }

        private void HandleGuestSignIn()
        {
            StartAuthenticating();
            OnGuestSignInRequested?.Invoke();
        }

        private void HandleContinueReturningPlayer()
        {
            SetNeedleAngle(maxNeedleAngle);
            OnContinueRequested?.Invoke();
        }

        public void StartAuthenticating()
        {
            _isAuthenticating = true;
            if (_overlaySigning != null) _overlaySigning.RemoveFromClassList("hidden");
            SetNeedleAngle(70f);
        }

        public void CancelAuthentication()
        {
            _isAuthenticating = false;
            HideOverlays();
            SetNeedleAngle(minNeedleAngle);
        }

        public void ShowError(string message, string code = "AUTH-12501")
        {
            _isAuthenticating = false;
            if (_overlaySigning != null) _overlaySigning.AddToClassList("hidden");
            if (_overlayError != null) _overlayError.RemoveFromClassList("hidden");

            if (_errorTextLabel != null) _errorTextLabel.text = message;
            if (_errorCodeLabel != null) _errorCodeLabel.text = $"CODE {code}";

            SetNeedleAngle(minNeedleAngle);
        }

        public void HideOverlays()
        {
            if (_overlaySigning != null) _overlaySigning.AddToClassList("hidden");
            if (_overlayError != null) _overlayError.AddToClassList("hidden");
        }

        private void HideErrorOverlay()
        {
            if (_overlayError != null) _overlayError.AddToClassList("hidden");
        }

        #endregion

        #region Progress & Tachometer

        public void SetDownloadProgress(float progress01, string textOverride = null)
        {
            float clamped = Mathf.Clamp01(progress01);
            int pct = Mathf.RoundToInt(clamped * 100f);

            if (_downloadPctLabel != null)
            {
                _downloadPctLabel.text = textOverride ?? $"DOWNLOADING ASSETS  {pct}%";
            }

            if (_downloadFill != null)
            {
                _downloadFill.style.width = Length.Percent(pct);
            }
        }

        public void SetNeedleAngle(float angle)
        {
            _targetNeedleAngle = Mathf.Clamp(angle, minNeedleAngle, maxNeedleAngle);
        }

        #endregion
    }
}