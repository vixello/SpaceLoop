using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Core;
using Cysharp.Threading.Tasks;
using System;
using VContainer;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Linq;

namespace Assets.Scripts.Core
{
    public class AdditiveScenesManager : MonoBehaviour
    {
        [Header("Scenes To Load")]
        [SerializeField] private AddressableSceneField _loginScene;
        [SerializeField] private AddressableSceneField _maineMenu;
        [SerializeField] private AddressableSceneField _loadingScene;
        private LoadingScreen _loadingScreen;
        private HashSet<string> _scenesToInitialize = new HashSet<string>();
        private EventBinding<SceneTransitionEvent> _sceneTransitionEvent;
        private EventBinding<FastSceneTransitionEvent> _fastSceneTransitionEvent;
        private EventBinding<LogoutSucceeded> _logoutBinding;
        private Func<ILoadingScope> _beginLoading;
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedScenes = new();

        [Inject]                       
        private void Construct(Func<ILoadingScope> func)
        {
            _beginLoading = func;
        }

        private void Awake()
        {
            _sceneTransitionEvent = new EventBinding<SceneTransitionEvent>(OnSceneTransition);
            _fastSceneTransitionEvent = new EventBinding<FastSceneTransitionEvent>(OnFastSceneTransition);
            _logoutBinding = new EventBinding<LogoutSucceeded>(OnLogout);
            EventBus<SceneTransitionEvent>.Register(_sceneTransitionEvent);
            EventBus<FastSceneTransitionEvent>.Register(_fastSceneTransitionEvent);
            EventBus<LogoutSucceeded>.Register(_logoutBinding);

            Debug.Log("Additivescenemaager ready");
        }

        private void OnDestroy()
        {
            EventBus<SceneTransitionEvent>.Deregister(_sceneTransitionEvent);
            EventBus<FastSceneTransitionEvent>.Deregister(_fastSceneTransitionEvent);
            EventBus<LogoutSucceeded>.Deregister(_logoutBinding);
        }


        public async UniTask StartGame()
        {
            //SceneManager.LoadSceneAsync(_bootstrap);
            //await SceneManager.LoadSceneAsync(_loginScene, LoadSceneMode.Additive);
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(_loginScene.SceneReference, LoadSceneMode.Additive);
            await handle.ToUniTask();
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load scene {_loginScene.SceneName}");
                return;
            }

            _loadedScenes[_loginScene.SceneReference.RuntimeKey.ToString()] = handle;
        }

        /*        private IEnumerator ProgressLoadingBar()
                {
                    float loadProgress = 0f;
                    for (int i = 0; i < ScenesToLoad.Count; ++i)
                    {
                        while (!ScenesToLoad[i].isDone)
                        {
                            loadProgress += ScenesToLoad[i].progress;
                            yield return null;
                        }
                    }
                }*/

        /*        private void OnTriggerEnter2D(Collider2D collision)
                {
                    if (collision.CompareTag("PLayer"))
                    {
                        LoadScenes();
                        UnloadScenes();
                    }
                }*/


        private void OnSceneTransition(SceneTransitionEvent @event)
        {
            _ = RunTransition(@event.ScenesToUnload, @event.ScenesToLoad);
        }

        public void OnFastSceneTransition(FastSceneTransitionEvent transition)
        {
            // Fire and forget (Unity doesn’t await)
            _ = RunFastTransition(transition.ScenesToUnload, transition.ScenesToLoad);
        }

        public void RunGameTransition(SceneTransitionSO transition)
        {
            // Fire and forget (Unity doesn’t await)
            _ = RunTransition(transition.ScenesToUnload, transition.ScenesToLoad);
        }

        public async UniTask RunTransition(ISceneField[] scenesToUnload, ISceneField[] scenesToLoad)
        {
            if (_loadingScene != null && !IsSceneLoaded(_loadingScene))
            {
                //await SceneManager.LoadSceneAsync(_loadingScene.SceneName, LoadSceneMode.Additive);
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(_loadingScene.SceneReference, LoadSceneMode.Additive);
                await handle.ToUniTask();

                _loadedScenes[_loadingScene.SceneReference.RuntimeKey.ToString()] = handle;

                _loadingScreen = GetLoadingScreen(_loadingScene);

                if (_loadingScreen == null)
                    Debug.LogWarning($"LoadingScreen not found in scene {_loadingScene.SceneName}");
            }

            using (var loadingScreenDisposable = new ShowLoadingScreenDisposable(_loadingScreen))
            {
                loadingScreenDisposable.SetLoadingBarPercent(0f);

                await UnloadScenes(scenesToUnload, loadingScreenDisposable);
                await LoadScenes(scenesToLoad, loadingScreenDisposable);
            }

            if (_loadingScene != null && IsSceneLoaded(_loadingScene))
            {
                AsyncOperationHandle<SceneInstance> handle = _loadedScenes[_loadingScene.SceneReference.RuntimeKey.ToString()];
                await Addressables.UnloadSceneAsync(handle);
                // await SceneManager.UnloadSceneAsync(_loadingScene.SceneName);
            }
        }

        private async UniTask RunFastTransition(ISceneField[] scenesToUnload, ISceneField[] scenesToLoad)
        {
            using (var loadingCircle = _beginLoading())
            {
                loadingCircle.SetMessage("");

                await UnloadScenes(scenesToUnload);
                await LoadScenes(scenesToLoad);
            }
        }

        private void OnLogout(LogoutSucceeded @event)
        {
            _ = HandleLogout();
        }

        private async UniTask HandleLogout()
        {
            var handlesToUnload = _loadedScenes
                 .Where(kvp => kvp.Value.IsValid())
                 .Where(kvp =>
                {
                    var sceneName = kvp.Value.Result.Scene.name;
                    return sceneName != SceneNames.Bootstrap &&
                           sceneName != SceneNames.LoginScene;
                })
                .Select(kvp => kvp.Value)
                .ToList();

            foreach (var handle in handlesToUnload)
            {
                Debug.Log($"[Logout] Unloading scene: {handle.Result.Scene.name}");
                await Addressables.UnloadSceneAsync(handle).ToUniTask();
            }

            _loadedScenes.Clear();

            // Ensure login scene is loaded
            if (!IsSceneLoaded(_loginScene))
            {
                var loginHandle = Addressables.LoadSceneAsync(
                    _loginScene.SceneReference,
                    LoadSceneMode.Additive
                );

                await loginHandle.ToUniTask();
                _loadedScenes[_loginScene.SceneReference.RuntimeKey.ToString()] = loginHandle;
            }
        }


        private async UniTask FakeLoad(ShowLoadingScreenDisposable loading, float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float p = Mathf.Lerp(from, to, t);
                loading.SetLoadingBarPercent(p);
                await UniTask.Yield();
            }

            loading.SetLoadingBarPercent(to);
        }

        private LoadingScreen GetLoadingScreen(ISceneField loadingScene)
        {
            string key = loadingScene.SceneReference.RuntimeKey.ToString();
            // Get a handle to that scene
            if (_loadedScenes.TryGetValue(key, out var loadedSceneHandle))
            {
                // Iterate all root GameObjects
                foreach (var rootObj in loadedSceneHandle.Result.Scene.GetRootGameObjects())
                {
                    if (rootObj.TryGetComponent<LoadingScreen>(out var screen))
                        return screen;
                }
            }

            Debug.LogWarning($"LoadingScreen not found in scene {loadingScene.SceneName}");
            return null;
        }

        private bool IsSceneLoaded(ISceneField scene)
        {
            return _loadedScenes.ContainsKey(scene.SceneReference.RuntimeKey.ToString());
        }

        public async UniTask LoadScenes(ISceneField[] scenesToLoad, ShowLoadingScreenDisposable loading = null)
        {
            _scenesToInitialize.Clear();

            int count = scenesToLoad.Length;
            float start = 0.5f;
            float end = 1f;
            float perScene = (end - start) / count;
            float duration = 0.3f;

            for (int i = 0; i < scenesToLoad.Length; ++i)
            {
                //bool isSceneLoaded = false;
                bool isSceneLoaded = _loadedScenes.ContainsKey(((ISceneField)scenesToLoad[i]).SceneReference.RuntimeKey.ToString());
                float endPercent = start + perScene * (i + 1);
                
                string sceneName = scenesToLoad[i].SceneName;
                ISceneField sceneField = scenesToLoad[i];
                Debug.Log($"Loading scene {sceneField.SceneName}");

                if (isSceneLoaded)
                {
                    string runTimeKey = sceneField.SceneReference.RuntimeKey.ToString();
                    AsyncOperationHandle<SceneInstance> handle = _loadedScenes[runTimeKey];

                    Scene loadedScene = handle.Result.Scene;
                    await InitializeScene(loadedScene, runTimeKey, () =>
                        AnimateLoading(start, endPercent, duration, loading));
                }

                if (!isSceneLoaded)
                {
                    AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneField.SceneReference, LoadSceneMode.Additive); /*SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);*/
                    await handle.ToUniTask();
                   
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"Failed to load scene {sceneField.SceneName}");
                        continue;
                    }

                    string runTimeKey = sceneField.SceneReference.RuntimeKey.ToString();
                    _loadedScenes[runTimeKey] = handle;
                    Scene loadedScene = handle.Result.Scene;

                    await InitializeScene(loadedScene, runTimeKey, () =>
                        AnimateLoading(start, endPercent, duration, loading));
                }
            }
        }

        public async Task UnloadScenes(ISceneField[] scenesToUnload, ShowLoadingScreenDisposable loading = null)
        {
            if(scenesToUnload.Length == 0) { return; }

            for (int i = 0; i < scenesToUnload.Length; ++i)
            {
                ISceneField sceneField = scenesToUnload[i];
                string key = sceneField.SceneReference.RuntimeKey.ToString();
                
                if (_loadedScenes.TryGetValue(key, out var handle))
                {
                    Debug.Log($"Unloading scene: {sceneField.SceneName}");

                    await Addressables.UnloadSceneAsync(handle).ToUniTask();
                    _loadedScenes.Remove(sceneField.SceneReference.RuntimeKey.ToString());

                    // Free memory 
                    await Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
            await AnimateLoading(0f, 0.5f, 0.2f, loading);
        }

        private async UniTask AnimateLoading(float start, float end, float duration, ShowLoadingScreenDisposable loading)
        {
            if (loading == null)
                return;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                loading.SetLoadingBarPercent(Mathf.Lerp(start, end, elapsed / duration));
                await UniTask.Yield();
            }

            loading.SetLoadingBarPercent(end);
        }

        private static async UniTask InitializeScene(Scene loadedScene, string runtimeKey, Func<UniTask> callback)
        {
            bool initializerFound = false;
            foreach (var root in loadedScene.GetRootGameObjects())
            {
                var init = root.GetComponentInChildren<SceneInitializer>();
                if (init != null)
                {
                    initializerFound = true;
                    await init.InitializeScene(runtimeKey, callback);
                }
            }
            if (!initializerFound)
            {
               await callback(); 
            }
        }

        private void AddToInitializationCheck(Scene loadedScene)
        {
            _scenesToInitialize.Add(loadedScene.name);
            EventBus<ScenesInitializationEvent>.Raise(new ScenesInitializationEvent { ScenesToInitialize = _scenesToInitialize });
        }
    }
}
