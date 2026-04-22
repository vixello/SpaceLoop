using Assets.Scripts.Services.Eos;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;

public class ProfileEditorScope : LifetimeScope
{
    private void Start()
    {
        var service = Container.Resolve<ISceneTransitionService>();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SceneInitializer>();
        builder.RegisterComponentInHierarchy<SceneTransitionButton>();
    }
}
