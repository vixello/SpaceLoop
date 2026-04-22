using UnityEngine;

namespace Assets.Scripts.Data.Titles
{
    [CreateAssetMenu(menuName = "Game/Title Config")]
    public class TitleConfig : ScriptableObject
    {
        public string TitleId;
        public string DisplayName;
        public string Description;
        public int RequiredLevel;
        public string RequiredAchievementId;
        public Material TMP_FontMaterial;
        public Color TitleColor;
    }
}
