using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core
{
    public interface IInitializer
    {
        public void RegisterAll(IInitializable[] initializables);
        public void Register(IInitializable initializable);
        public void Unregister(IInitializable initializable);
        public UniTask InitializeInitializables(Action<string> logCallback = null);
    }

    public class GameInitializer : BaseInitializer 
    {
        public GameInitializer()
        {
            UnityEngine.Debug.Log("[GameInitializer] ctor called");
        }
    }

    public class BaseInitializer : IInitializer
    {
        protected HashSet<IInitializable> _initializables = new HashSet<IInitializable>();
        public bool IsInitialized { get; private set; }

        public void RegisterAll(params IInitializable[] initializables)
        {
            foreach (var initializable in initializables)
            {
                Register(initializable);
            }
        }

        public void Register(IInitializable initializable)
        {
            if (!_initializables.Contains(initializable))
            {
                _initializables.Add(initializable);
                UnityEngine.Debug.Log($"[BaseInitializer] regstered {initializable.GetType()}");
            }
        }

        public void Unregister(IInitializable initializable)
        {
            if (_initializables.Contains(initializable))
            {
                _initializables.Remove(initializable);
            }
        }

        public async UniTask InitializeInitializables(Action<string> logCallback = null)
        {
            foreach (IInitializable initializable in _initializables)
            {
                if (initializable != null /*&& !initializable.IsInitailized()*/)
                {
                    logCallback?.Invoke($"[Initializer] initializing {initializable.GetType()}...");
                    try
                    {
                        await initializable.Initialize();
                    }
                    catch (Exception ex)
                    {
                        logCallback?.Invoke($"[Initializer] {initializable.GetType()} crashed: {ex}");
                    }
                    logCallback?.Invoke($"[Initializer] {initializable.GetType()} initialized!");
                }
            }
            //await WaitForAllInitializables();
            _initializables.Clear();
        }

        public async UniTask WaitForAllInitializables()
        {
            while (!_initializables.All(i => i.IsInitialized()))
            {
                await UniTask.Yield();
            }
        }
    }
}
