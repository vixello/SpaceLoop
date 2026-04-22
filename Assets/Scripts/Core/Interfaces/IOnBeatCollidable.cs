using UnityEngine;

namespace Assets.Scripts.Core
{
    public interface IOnBeatCollidable
    {
        public void CalculatePositionToCollideOnBeat(GameObject obj, float objectSpeed, ref Vector3 objectStartPosition, float defaultSpawnPositionX = 0f);
    }
}