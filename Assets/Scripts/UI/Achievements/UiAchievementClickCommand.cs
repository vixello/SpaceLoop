using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.UI
{
    public class UIAchievementClickCommand : ICommand<AchievementCommandContext>
    {
        private readonly ISceneTransitionService _sceneTransitionService;

        [Inject]
        public UIAchievementClickCommand(ISceneTransitionService sceneTransitionService)
        {
            _sceneTransitionService = sceneTransitionService;
        }

        public async UniTask Execute(AchievementCommandContext context)
        {
            Debug.Log($"[ACHIEV] UIAchievementClickCommand RunFastTransition {context.SceneTransition}");
            await _sceneTransitionService.RunFastTransition(context.SceneTransition);
            Debug.Log($"[ACHIEV] UIAchievementClickCommand {context.IsAssigned}");

            if (!context.IsAssigned)
            {
                Debug.Log("[ACHIEV] UIAchievementClickCommand Fire event EnableAchievementPinningMode");
                EventBus<EnableAchievementPinningMode>.Raise(
                    new EnableAchievementPinningMode()
                );
            }
            else
            {
                Debug.Log($"[ACHIEV] AchievementDetailRequested");
                EventBus<AchievementDetailRequested>.Raise(
                    new AchievementDetailRequested
                    {
                        AchievementId = context.AchievementId,
                    }
                );
            }
        }
    }

}
