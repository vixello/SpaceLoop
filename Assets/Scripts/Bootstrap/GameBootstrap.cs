using Assets.Scripts.Core;
using Core;
using PlayEveryWare.EpicOnlineServices;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;
using Assets.Scripts.Services.Auth;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Systems;
using Assets.Scripts.Services.Cloud;
using Assets.Scripts.Services.Achievements;
using Assets.Scripts.Data.Level;
using Assets.Scripts.Services.EOS.Cloud;
using Assets.Scripts.Data.Rewards;

namespace Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bindable Objects")]
        [SerializeField] private List<Object> _bindables;
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private Camera _camera;
        // [SerializeField] protected Canvas _loadingCircle;

        private GameManager _gameManager;
        private UpdatePublisher _updatePublisher;
        private AdditiveScenesManager _additiveScenesManager;
        private EOSManager _eosManager;
        private LoginManager _eosLoginManager;
        private CloudSyncService _cloudSyncEOS;
        private IInitializer _initializer;
        private ISaveSystem _saveSystem;
        private AchievementsManager _achievementsManager;
        private ISceneTransitionService _sceneTransitionService;
        private LevelCurve _levelCurve;
        private PlayerLevelSystem _playerLevelSystem;
        private ITitleManager _titleManager;
        private StatServiceMiddleman _statServiceMiddleman;
        private IProfileEditorManager _profileEditorManager;
        private RewardVisualDatabase _rewardVisualDatabase;

        private async void Start()
        {
            BindObjects();

            using (var loadingScreenDisposable =
                new ShowLoadingScreenDisposable(_loadingScreen))
            {
                using(var cts = new CancellationTokenSource())
                {
                    loadingScreenDisposable.SetLoadingBarPercent(0f);
                    await InitializeObjects(cts.Token);
                    loadingScreenDisposable.SetLoadingBarPercent(0.33f);
                    await CreateObjects(cts.Token);
                    loadingScreenDisposable.SetLoadingBarPercent(0.66f);
                    await PrepareGame(cts.Token);
                    loadingScreenDisposable.SetLoadingBarPercent(1.0f);
                }
            }

            await BeginGame();
        }

        [Inject]
        private void Construct(GameManager gameManager,
                                UpdatePublisher updatePublisher,
                                AdditiveScenesManager additiveScenesManager,
                                EOSManager eosManager,
                                LoginManager eosLoginManager,
                                CloudSyncService cloudSyncServiceEOS,
                                IInitializer initializer,
                                ISaveSystem saveSystem,
                                AchievementsManager achievementsManager,
                                ISceneTransitionService sceneTransitionService,
                                LevelCurve levelCurve,
                                PlayerLevelSystem playerLevelSystem,
                                ITitleManager titleManager,
                                StatServiceMiddleman statServiceMiddleman,
                                IProfileEditorManager profileEditorManager,
                                RewardVisualDatabase rewardVisualDatabase
                                )
        {
            Debug.Log("[BOOTSTRAP] construct");
            _gameManager = gameManager;
            _updatePublisher = updatePublisher;
            _additiveScenesManager = additiveScenesManager;
            _eosManager = eosManager;
            _eosLoginManager = eosLoginManager;
            _cloudSyncEOS = cloudSyncServiceEOS;
            _initializer = initializer;
            _saveSystem = saveSystem;
            _achievementsManager = achievementsManager;
            _sceneTransitionService = sceneTransitionService;
            _levelCurve = levelCurve; 
            _playerLevelSystem = playerLevelSystem;
            _titleManager = titleManager;
            _statServiceMiddleman = statServiceMiddleman;
            _profileEditorManager = profileEditorManager;
            _rewardVisualDatabase = rewardVisualDatabase;
        }

        private void BindObjects()
        {
            foreach(IBindableObject bindable in _bindables)
            {
                bindable.BindObject();
            }
        }

        private async UniTask InitializeObjects(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (_initializer == null)
            {
                Debug.LogError("[BOOTSTRAP] No Initializer found!");
            }
            else
            {
                RegisterInitializables();
                await _initializer.InitializeInitializables(msg => Debug.Log(msg));
            }

            await _rewardVisualDatabase.Initialize();
            Debug.Log("[BOOTSTRAP] Initializers completed!");
        }

        private void RegisterInitializables()
        {
            Debug.Log($"[GAME INITIALIZER] registering {_cloudSyncEOS.GetType()}");
            _initializer?.Register(_cloudSyncEOS);
            _initializer?.Register(_titleManager);
            _initializer?.Register(_statServiceMiddleman);
        }

        private async UniTask CreateObjects(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
        }

        private async UniTask PrepareGame(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _saveSystem.Register(_achievementsManager);
            _saveSystem.Register(_playerLevelSystem);
            _saveSystem.Register(_titleManager);
            _saveSystem.Register(_profileEditorManager);
            await _saveSystem.Load();

            await _cloudSyncEOS.RequestSync(CloudSyncReason.ConnectivityRestored, CloudProviderType.Eos);
            await UniTask.Yield();
        }

        private async UniTask BeginGame()
        {
            if (_additiveScenesManager != null)
            {
                await _additiveScenesManager.StartGame();
                Destroy(_camera);
            }
            else
            {
                Debug.LogError("AdditiveScenesManager is not initialized.");
            }
        }
    }
}
