using Assets.Scripts.Services.Eos;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Assets.Scripts.Services.EOS.Auth;
using Assets.Scripts.Core;
using Assets.Scripts.UI;
using Assets.Scripts.Core.Interfaces;

public class MainMenuScope : LifetimeScope
{
    [SerializeField] private UILoginMenuEOS _uiLoginMenuEOS;

    protected override async void Awake()
    {
        base.Awake();
    }

    private async void Start()
    {
        var provider = Container.Resolve<ILoginBackendProvider>();
        provider.SetBackend(_uiLoginMenuEOS);

        var googleAuth = Container.Resolve<GoogleAuthProvider>();
        var deviceAuth = Container.Resolve<DeviceAuthProvider>();
        var sceneInitializer = Container.Resolve<SceneInitializer>();

        sceneInitializer.Register(googleAuth);
        sceneInitializer.Register(deviceAuth);
        await sceneInitializer.InitializeScene();

        var service = Container.Resolve<ISceneTransitionService>();
        Debug.Log($"MainMenuScope can resolve service: {service.GetType()}");
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SceneInitializer>();
        builder.RegisterComponentInHierarchy<UIUsernameValidator>();
        builder.RegisterComponentInHierarchy<SceneTransitionButton>();
        builder.RegisterComponentInHierarchy<UIAchievementPinned>();
        // Register the UI backend for injection into scene-local components
        //builder.RegisterComponent(_uiLoginMenuEOS).As<UILoginMenuEOS>();
    }
}