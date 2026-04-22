using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.UI
{
    public class DialogFactory : MonoBehaviour
    { 
        private readonly Dictionary<Type, DialogBase> _cache = new Dictionary<Type, DialogBase>();

        public async UniTask<T> Get<T>() where T : DialogBase
        {
            var type = typeof(T);

            if(_cache.TryGetValue(type, out var dialog))
            {
                if(dialog == null) { _cache.Remove(type); }
                else
                {
                    return (T)dialog;
                }
            }

            T instance = FindFirstObjectByType<T>(findObjectsInactive: FindObjectsInactive.Include);
            if(instance != null)
            {
                _cache[type] = instance;
                return instance;
            }

            string address = $"Assets/Prefabs/UI/{type.Name}.prefab";
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            GameObject prefab = await handle.ToUniTask();

            GameObject go = Instantiate(prefab); 
            instance = go.GetComponent<T>();

            if (instance == null) throw new Exception($"Prefab at {address} does not contain component {type.Name}");

            _cache[type] = instance;
            return instance;
        }
    }
}

