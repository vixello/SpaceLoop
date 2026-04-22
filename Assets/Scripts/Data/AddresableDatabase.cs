using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Assets.Scripts.Data
{
    public class AddresableDatabase<T> : ScriptableObject where T : UnityEngine.Object
    {
        [Header("Reference IDs, not loaded")]
        public List<string> Ids = new List<string>();

        [Header("Addressable path for each ID")]
        public Dictionary<string, string> ConfigAddresses = new Dictionary<string, string>();

        protected Dictionary<string, T> _loadedCache = new Dictionary<string, T>();

        [SerializeField] protected string _databaseLabel = "Spaceship";

        /// <summary>
        /// Populate Ids and ConfigAddresses from Addressables label
        /// </summary>
        public async UniTask PopulateFromAddresablesLabel(string label = null)
        {
            label ??= _databaseLabel;

            Ids.Clear();
            ConfigAddresses.Clear();

            IList<IResourceLocation> resourceLocations = await Addressables.LoadResourceLocationsAsync(label).Task;

            foreach (var resourceLocation in resourceLocations)
            {
                string path = resourceLocation.PrimaryKey;
                string shortID = System.IO.Path.GetFileNameWithoutExtension(path);

                Ids.Add(path);
                ConfigAddresses.Add(path, shortID);
            }

            Debug.Log($"Found {Ids.Count} addressables with label '{label}'");
        }

        public async UniTask<T> GetAsync(string id)
        {
            if (_loadedCache.TryGetValue(id, out T cached))
            {
                return cached;    
            }

            if (!ConfigAddresses.TryGetValue(id, out string address))
            {
                Debug.LogError($"ID not found in addresses: {id}");
                return null;
            }

            T asset = await Addressables.LoadAssetAsync<T>(address).Task;
            _loadedCache[id] = asset;
            return asset;
        }

        public T GetCached(string id)
        {
            _loadedCache.TryGetValue(id, out var cached);
            return cached;
        }

        public void Release(string id)
        {
            if (_loadedCache.TryGetValue(id, out var asset))
            {
                Addressables.Release(asset);
                _loadedCache.Remove(id);
            }
        }

        public void ReleaseAll()
        {
            foreach (var kv in _loadedCache)
            {
                Addressables.Release(kv.Value);
            }
            _loadedCache.Clear();
        }
    }
}
