using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public class ScenesStateManager 
    {
        private EventBinding<ScenesInitializationEvent> _sceneInitializationBinding;
        private EventBinding<SceneLoadedEvent> _sceneLoadedEvent;
        private HashSet<string> _scenesToInitialize = new HashSet<string>();

        public ScenesStateManager() 
        {
            _sceneInitializationBinding = new EventBinding<ScenesInitializationEvent>(OnSceneInitialization);
            _sceneLoadedEvent = new EventBinding<SceneLoadedEvent>(OnSceneLoaded);

            EventBus<ScenesInitializationEvent>.Register(_sceneInitializationBinding);
            EventBus<SceneLoadedEvent>.Register(_sceneLoadedEvent);
        }

        ~ScenesStateManager()
        {
            EventBus<ScenesInitializationEvent>.Deregister(_sceneInitializationBinding);
            EventBus<SceneLoadedEvent>.Deregister(_sceneLoadedEvent);
        }

        private async UniTask WaitForScenesToinitialize()
        {
            while (_scenesToInitialize.Count > 0)
            {
                await UniTask.Yield();
            }

            EventBus<ChangeGameStateEvent>.Raise(new ChangeGameStateEvent { GameState = GameState.Start }); 
        }

        private void OnSceneLoaded(SceneLoadedEvent e)
        {
            _scenesToInitialize.Remove(e.SceneName);
        }

        private async void StartWaiting()
        {
            await WaitForScenesToinitialize();
        }

        private void OnSceneInitialization(ScenesInitializationEvent e)
        {
            _scenesToInitialize = e.ScenesToInitialize;
            StartWaiting();
        }
    }
}
