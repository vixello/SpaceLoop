using UnityEngine;

namespace Assets.Scripts.Data.Spaceship
{

    [System.Serializable]

    [CreateAssetMenu(menuName = "Gameplay/Spaceship Config")]
    public class SpaceshipConfig : ScriptableObject, ISpaceship
    {
        [SerializeField] private string _id;
        [SerializeField] private GameObject _shipPrefab;
        [SerializeField] private float _shipMaxSpeed = 50f;
        [SerializeField] private float _shipAcceleration = 20f;
        [SerializeField] private float _shipTurnSpeed = 60f;

        public string ID => _id;
        public GameObject Prefab => _shipPrefab;
        public float MaxSpeed => _shipMaxSpeed;
        public float Acceleration => _shipAcceleration;
        public float TurnSpeed => _shipTurnSpeed;
    }
}
