using Core;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;
using Assets.Scripts.Systems;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Services.Auth;
using VContainer;
using VContainer.Unity;
using Assets.Scripts.Services.EOS.Auth;
using Assets.Scripts.UI;
using Assets.Scripts.Services.EOS;
using Assets.Scripts.Services.Cloud;
using Assets.Scripts.Services.Achievements;
using System.Collections.Generic;
using Data;
using Assets.Scripts.Data.Titles;
using Assets.Scripts.Data.Level;
using Assets.Scripts.Services.EOS.Cloud;
using Assets.Scripts.Data;
using Assets.Scripts.Data.Rewards;
using Assets.Scripts.Data.Spaceship;
using Assets.Scripts.Gameplay;

namespace Bootstrap
{
    [DefaultExecutionOrder(-100)]
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private UpdatePublisher _updatePublisher;
        [SerializeField] private AdditiveScenesManager _additiveScenesManager;
        [SerializeField] private EOSManager _eosManager;
        //[SerializeField] private UILoginMenuEOS _uiLoginMenu;
        [SerializeField] private LoginManager _loginManager;
        [SerializeField] private DialogFactory _dialogFactory;
        [SerializeField] private AchievementsManager _achievementsManager;
        [SerializeField] private ProfilePictureDatabase _profilePictureDatabase;
        [SerializeField] private TitleDatabase _titleDatabase; 
        [SerializeField] private RewardVisualDatabase _rewardVisualDatabase; 
        [SerializeField] private TitleManager _titleManager;
        [SerializeField] private LevelCurve _levelCurve;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private AddresableDatabase<SpaceshipConfig> _spaceShipDatabase;

        protected override void Configure(IContainerBuilder builder)
        {
            // Managers
            builder.RegisterComponentInNewPrefab(_gameManager, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_updatePublisher, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_additiveScenesManager, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_eosManager, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_loginManager, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_dialogFactory, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_achievementsManager, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_titleManager, Lifetime.Singleton).As<ITitleManager>(); 
            builder.Register<IIdentityManager,IdentityManager>(Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_inputManager, Lifetime.Singleton);
            builder.Register<PlayerSpaceshipManager>(Lifetime.Singleton);

            // Saveables
            builder.RegisterEntryPoint<WalletManager>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ProfileEditorManager>(Lifetime.Singleton).As<IProfileEditorManager>();
            builder.Register<ISaveSystem, SaveSystem>(Lifetime.Singleton);

            // Databases
            builder.RegisterInstance(_profilePictureDatabase);
            builder.RegisterInstance(_titleDatabase);
            builder.RegisterInstance(_rewardVisualDatabase);
            builder.RegisterInstance(_spaceShipDatabase).As<AddresableDatabase<SpaceshipConfig>>();

            // Systems
            builder.Register<PlayerLevelSystem>(Lifetime.Singleton);
            builder.Register<IUsernameValidationSystem, UsernameValidationSystem>(Lifetime.Singleton);
            
            // Initialization
            builder.Register<IInitializer, GameInitializer>(Lifetime.Singleton);
            builder.Register<ScenesStateManager>(Lifetime.Singleton);

            // Services
            builder.Register<IDialogService, DialogService>(Lifetime.Singleton);
            builder.Register<GoogleAuthProvider>(Lifetime.Singleton);
            builder.Register<DeviceAuthProvider>(Lifetime.Singleton);
            builder.Register<IAuthProviderFactory, EosAuthProviderFactory>(Lifetime.Singleton);
            builder.Register<ILoginBackendProvider, LoginBackendProvider>(Lifetime.Singleton);
            builder.Register<SceneTransitionService>(Lifetime.Singleton)
                   .AsImplementedInterfaces()   // exposes ISceneTransitionService and IStartable
                   .AsSelf();
            builder.RegisterFactory<Dictionary<string, LocalAchievementSaveData>, uint, PinnedAchievementManager>(
                resolver => (localData, maxPinned) => new PinnedAchievementManager(localData, maxPinned),
                Lifetime.Transient
            );

            //Cloud service
            builder.Register<ICloudSyncProviderFactory, CloudSyncProviderFactory>(Lifetime.Singleton);
            builder.Register<ICloudSyncProvider, EosCloudSyncProvider>(Lifetime.Singleton);
            builder.Register<CloudSyncService>(Lifetime.Singleton);

            //builder.Register<EOSLoginService>(Lifetime.Singleton);
            //builder.Register<ILoginService>(Lifetime.Singleton, sp => sp.Resolve<EOSLoginService>());

            // Bootstrap
            builder.RegisterComponentInHierarchy<GameBootstrap>();
            builder.RegisterComponentInHierarchy<LoadingCanvasProvider>();

            // Ui 
            builder.Register<Canvas>(resolver =>
                resolver.Resolve<LoadingCanvasProvider>().Canvas, 
                Lifetime.Singleton
            );
            builder.RegisterFactory<ILoadingScope>(
                resolver => () => new LoadingCircle(resolver.Resolve<Canvas>()),
                Lifetime.Singleton
            );
            builder.Register<UIAchievementClickCommand>(Lifetime.Transient);

            builder.Register<ICommandFactory<UIAchievementClickCommand>>(
                resolver => new CommandFactory<UIAchievementClickCommand>(resolver),
                Lifetime.Singleton
            );

            // Other
            builder.RegisterInstance(_levelCurve);
            builder.Register<StatServiceMiddleman>(Lifetime.Singleton);
        }
    }
}
