using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Saveables/Profile Picture Database")]
    public class ProfilePictureDatabase : ScriptableObject
    {
        public List<ProfilePictureConfig> Pictures;
    }
}
