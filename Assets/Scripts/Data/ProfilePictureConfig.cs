using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Saveables/Profile Picture Config")]
    public class ProfilePictureConfig : ScriptableObject
    {
        public Sprite Image;
        public int LevelToUnlock;
        public string Id;
    }
}
