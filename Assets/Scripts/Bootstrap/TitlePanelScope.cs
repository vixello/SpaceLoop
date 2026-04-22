using VContainer;
using VContainer.Unity;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.UI.PlayerIdentity;

public class TitlePanelScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SceneInitializer>();
        builder.RegisterComponentInHierarchy<UITitleController>();
    }
}