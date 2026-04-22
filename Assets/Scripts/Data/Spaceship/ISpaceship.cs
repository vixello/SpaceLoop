using UnityEngine;

namespace Assets.Scripts.Data.Spaceship
{
    public interface ISpaceship
    {
        public string ID { get; }
        public GameObject Prefab { get; } 
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public float TurnSpeed { get; }
    }
}
