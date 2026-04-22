using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public enum HoverPointType
    {
        Core,
        Engine
    }

    public class HoverPoint : MonoBehaviour
    {
        public Transform point;
        public Transform visual;  // engine mesh (NO rigidbody)
        public float forceMultiplier = 1f;
        public float hoverHeight = 2f;
        public HoverPointType Type;
    }
}
