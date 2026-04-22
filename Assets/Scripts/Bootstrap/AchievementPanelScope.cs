using Assets.Scripts.Services.Eos;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;

public class AchievementPanelScope : LifetimeScope
{
    private void Start()
    {
        Debug.Log("AchievementPanelScope Start called"); 
        var service = Container.Resolve<ISceneTransitionService>();
        Debug.Log($"AchievementPanelScope can resolve service: {service.GetType()}");
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SceneTransitionButton>();
        builder.RegisterComponentInHierarchy<IUIAchievement>();
    }
}