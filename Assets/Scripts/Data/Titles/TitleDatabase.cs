using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Titles
{
    [CreateAssetMenu(menuName = "Game/Title Database")]
    public class TitleDatabase : ScriptableObject
    {
        public List<TitleConfig> Titles;

        private Dictionary<string, TitleConfig> _byId;

        public void Init()
        {
            _byId = new Dictionary<string, TitleConfig>();
            foreach (var t in Titles)
            {
                _byId[t.TitleId] = t;
            }
        }

        public TitleConfig GetById(string titleId)
        {
            if (_byId == null)
                Init();

            _byId.TryGetValue(titleId, out var config);
            return config;
        }

        public IEnumerable<TitleConfig> GetAll() => Titles;
    }
}
