using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using VContainer;

namespace Assets.Scripts.UI
{
    [System.Serializable]
    public class AssetReferenceVisualTreeAsset : UnityEngine.AddressableAssets.AssetReferenceT<VisualTreeAsset>
    {
        public AssetReferenceVisualTreeAsset(string guid) : base(guid) { }
    }

    public class UIScreenManager : MonoBehaviour, IUIScreenManager
    {
        [System.Serializable]
        public struct ScreenConfig
        {
            public ScreenType type;
            public AssetReferenceVisualTreeAsset uxmlAssetRef; // Holds the UIDocument for that screen
        }

        [SerializeField] private UIDocument _mainUIDocument;
        [SerializeField] private UIDocument _loadingUIDocument;
        [SerializeField] private List<ScreenConfig> _screenConfigs = new List<ScreenConfig>();
        [SerializeField] private List<UIScreenTransition> _transitions = new List<UIScreenTransition>();
        
        private Dictionary<ScreenType, AssetReference> _screenLookup = new Dictionary<ScreenType, AssetReference>();
        private Dictionary<VisualTreeAsset, UIScreenTransition> _transitionLookup = new Dictionary<VisualTreeAsset, UIScreenTransition>();

        private AsyncOperationHandle<VisualTreeAsset> _currentLoadHandle;
        private AsyncOperationHandle<VisualTreeAsset> _loadingLoadHandle;

        private ScreenType _currentScreen = ScreenType.Bootstrap;
        private ScreenType _currentLoadingScreen = ScreenType.Bootstrap;
        private Stack<ScreenType> _historyStack = new Stack<ScreenType>();

        private IUIControllersFactory _uiControllersFactory;
        private UIController _activeController;

        public UIDocument MainUIDocument => _mainUIDocument;
        public UIDocument LoadingUIDocument => _loadingUIDocument;
        public ScreenType CurrentLoadingScreen => _currentLoadingScreen;

        private void Awake()
        {
            BuildScreenLookup();
            BuildTransitionLookup();
            LoadScreenAddresable(_currentScreen, false, true);
        }

        [Inject]
        private void Construct(IUIControllersFactory uiControllersFactory)
        {
            _uiControllersFactory = uiControllersFactory;
        }

        private void BuildScreenLookup()
        {
            _screenLookup.Clear();
            foreach (ScreenConfig config in _screenConfigs)
            {
                if(config.uxmlAssetRef == null)
                {
                    Debug.LogWarning($"[UIScreenManager] AssetReference for {config.type} is unassigned!", this);
                    continue;
                }

                if (_screenLookup.ContainsKey(config.type))
                {
                    Debug.LogError($"[UIScreenManager] Duplicate ScreenType detected: {config.type}!", this);
                    continue;
                }

                _screenLookup.Add(config.type, config.uxmlAssetRef);
            }
        }

        private void BuildTransitionLookup()
        {
            _transitionLookup.Clear();
            foreach (var transition in _transitions)
            {
                if (transition != null && transition.UxmlAsset != null)
                {
                    if (!_transitionLookup.ContainsKey(transition.UxmlAsset))
                    {
                        _transitionLookup.Add(transition.UxmlAsset, transition);
                    }
                }
            }
        }

        public void ClearHistory()
        {
            if(_historyStack.Count > 0)
                _historyStack.Clear();
        }

        public void GoBackInHistory()
        {
            if(_historyStack.Count > 0 )
            {
                ScreenType previousScreen = _historyStack.Pop();
                SwitchScreen(previousScreen, true);
            }
            else
            {
                Debug.Log("[UIScreenManager] No screens left in history stack.");
            }
        }

        public void SwitchScreen(ScreenType targetScreenType, bool isGoingBack = false, bool clearHistory = false)
        {
            if (_currentScreen == targetScreenType) return;

            LoadScreenAddresable(targetScreenType, isGoingBack, clearHistory);
        }

        private void LoadScreenAddresable(ScreenType targetScreenType, bool isGoingBack = false, bool clearHistory = false)
        {
            if (!_screenLookup.TryGetValue(targetScreenType, out AssetReference assetRef))
            {
                Debug.LogError($"[UIScreenManager] ScreenType {targetScreenType} not found in lookup!");
                return;
            }

            if (_currentLoadHandle.IsValid())
            {
                Addressables.Release(_currentLoadHandle);
            }

            _currentScreen = targetScreenType;

            _currentLoadHandle = assetRef.LoadAssetAsync<VisualTreeAsset>();
            _currentLoadHandle.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    VisualTreeAsset loadedUxml = handle.Result;

                    if (_mainUIDocument != null)
                    {
                        _mainUIDocument.visualTreeAsset = loadedUxml;
                        BindScreenButtons(loadedUxml);
                    }


                    if (clearHistory) { ClearHistory(); }
                    if (!isGoingBack) { _historyStack.Push(targetScreenType); }

                    _activeController?.OnHide();

                    if (_uiControllersFactory.TryGetController(targetScreenType, out UIController controller))
                    {
                        _activeController = controller;
                        _activeController.Initialize(_mainUIDocument);
                        _activeController.OnShow();
                    }
                }
                else
                {
                    Debug.LogError($"[UIScreenManager] Failed to load screen asset for {targetScreenType}");
                }
            };
        }

        private void BindScreenButtons(VisualTreeAsset loadedUxml)
        {
            if (_mainUIDocument == null) return;

            VisualElement root = _mainUIDocument.rootVisualElement;
            if (root == null) return;

            if(_transitionLookup.TryGetValue(loadedUxml, out UIScreenTransition transitionConfig))
            {
                foreach(var entry in transitionConfig.ButtonTransitions)
                {
                    if (string.IsNullOrEmpty(entry.buttonId)) continue;
                    
                    Button button = root.Q<Button>(entry.buttonId);
                    if(button != null)
                    {

                        button.clicked -= GoBackInHistory;
                        ScreenType destination = entry.targetScreen;

                        if(entry.action == TransitionAction.NavigateToScreen)
                        {
                            button.clicked += () => SwitchScreen(destination);
                        }
                        else if(entry.action == TransitionAction.GoBack)
                        {
                            button.clicked += () => GoBackInHistory();
                        }
                        else if (entry.action == TransitionAction.ClearHistory)
                        {
                            button.clicked += () => SwitchScreen(destination, false, true);
                        }
                    }
                }
            }
        }

        public void ShowLoadingOverlay(ScreenType loadingScreenType)
        {
            if (_currentLoadingScreen == loadingScreenType)
            {
                if(!_loadingUIDocument.gameObject.activeSelf) _loadingUIDocument.gameObject.SetActive(true);
                return;
            }

            if (!_screenLookup.TryGetValue(loadingScreenType, out AssetReference assetRef))
            {
                Debug.LogError($"[UIScreenManager] Loading ScreenType {loadingScreenType} not found in lookup!");
                return;
            }

            _loadingUIDocument.gameObject.SetActive(true);

            if (_loadingLoadHandle.IsValid())
            {
                if (_loadingLoadHandle.IsDone)
                {
                    if (_loadingLoadHandle.Result != null)
                    {
                        _loadingUIDocument.visualTreeAsset = _loadingLoadHandle.Result;
                    }
                    return;
                }
                return; // Already loading
            }

            // Use static Addressables load method instead of assetRef.LoadAssetAsync()
            _loadingLoadHandle = Addressables.LoadAssetAsync<VisualTreeAsset>(assetRef);
            _loadingLoadHandle.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadingUIDocument.visualTreeAsset = handle.Result;
                }
                else
                {
                    Debug.LogError($"[UIScreenManager] Failed to load loading overlay asset for {loadingScreenType}");
                }
            };
        }

        public void HideLoadingOverlay()
        {
            if (_loadingLoadHandle.IsValid())
            {
                Addressables.Release(_loadingLoadHandle);
            }
            if (_loadingUIDocument != null)
            {
                _loadingUIDocument.visualTreeAsset = null;
                _loadingUIDocument.gameObject.SetActive(false);
            }
        }
    }
}
