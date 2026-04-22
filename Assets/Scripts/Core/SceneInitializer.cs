using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Core
{
    public class SceneInitializer : MonoBehaviour
    {
        [Header("Initialize Objects")]
        [SerializeField] private List<UnityEngine.Object> _initializableObjects;
        public readonly BaseInitializer Initializer = new BaseInitializer();

        private void Awake()
        {
            RegisterInitializableObjects();
        }

        public void Register(IInitializable initializable)
        {
            Initializer.Register(initializable);
        }

        public void Unregister(IInitializable initializable)
        {
            Initializer.Unregister(initializable);
        }

        public async UniTask InitializeScene(string runtimeKey = null, Func<UniTask> progressCallback = null)
        {
            Debug.Log($"[SceneInitializer] initializing scene {gameObject.scene.name}...");
            await Initializer.InitializeInitializables(msg => Debug.Log(msg));

            if (progressCallback != null)
                await progressCallback();

            EventBus<SceneLoadedEvent>.Raise(new SceneLoadedEvent { SceneName = gameObject.scene.name, RuntimeKey = runtimeKey });
        }

        public void RegisterInitializableObjects()
        {
            foreach(var obj in _initializableObjects)
            {
                if (obj is GameObject gammeObject)
                {
                    IInitializable initializable = gammeObject.GetComponent<IInitializable>();
                    Register(initializable);
                }
                else if (obj is IInitializable initializable)
                {
                    Register(initializable);
                }
                else
                {
                    Debug.LogWarning($"{obj.name} does not implement IInitializable");
                }
            }
        }
    }
}
