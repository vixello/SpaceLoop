using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Core
{
    public class SceneTransitionButton : MonoBehaviour
    {
        private ISceneTransitionService _sceneTransitionService;

        [Inject]
        private void Construct(ISceneTransitionService sceneTransitionService)
        {
            if (sceneTransitionService == null) { return; }
            _sceneTransitionService = sceneTransitionService;
        }

        public void RunFastTransition(SceneTransitionSO transition)
        {
            _sceneTransitionService.RunFastTransition(transition);
        }
    }
}

