using Assets.Scripts.Services.Eos;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Assets.Scripts.Services.EOS.Auth;
using Assets.Scripts.Core;

public class LoginLifetimeScope : LifetimeScope
{
    [SerializeField] private UILoginMenuEOS _uiLoginMenuEOS;

    protected override async void Awake()
    {
        base.Awake(); 
        var provider = Container.Resolve<ILoginBackendProvider>();
        provider.SetBackend(_uiLoginMenuEOS);

        var googleAuth = Container.Resolve<GoogleAuthProvider>();
        var deviceAuth = Container.Resolve<DeviceAuthProvider>();
        var sceneInitializer = Container.Resolve<SceneInitializer>();

        sceneInitializer.Register(googleAuth);
        sceneInitializer.Register(deviceAuth);
        await sceneInitializer.InitializeScene();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SceneInitializer>();
        // Register the UI backend for injection into scene-local components
        //builder.RegisterComponent(_uiLoginMenuEOS).As<UILoginMenuEOS>();
    }
}
