using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Assets.Scripts.Contracts;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Assets.Scripts.Data.Rewards
{
    [CreateAssetMenu(menuName = "Game/Reward Visual Database")]
    public class RewardVisualDatabase : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public RewardType Type;
            public AssetReference SpriteAtlas;   // addressable atlas
            public string SpriteName;             // name inside atlas
        }

        [SerializeField] private Entry[] _entries;
        private Dictionary<RewardType, Sprite> _loaded;
        private List<AsyncOperationHandle> _handles = new();

        public async UniTask Initialize()
        {
            _loaded = new();

            foreach (var e in _entries)
            {
                var atlasHandle = e.SpriteAtlas.LoadAssetAsync<SpriteAtlas>();
                _handles.Add(atlasHandle);

                SpriteAtlas atlas = await atlasHandle.Task.AsUniTask();

                if (atlas == null)
                {
                    Debug.LogError($"Atlas failed to load for reward type {e.Type}");
                    continue;
                }

                Sprite sprite = atlas?.GetSprite(e.SpriteName);
                if (sprite == null) 
                { 
                    Debug.LogError($"Sprite '{e.SpriteName}' not found in atlas for reward type {e.Type}"); 
                    continue; 
                }
                _loaded[e.Type] = sprite;
            }
        }

        public Sprite GetIcon(RewardType type)
            => _loaded.TryGetValue(type, out var s) ? s : null;
    }

}
