using Assets.Scripts.Core;
using Assets.Scripts.Gameplay.Music;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.Gameplay.Obstacles
{
    public class ObstacleBasic : ObstacleBehaviour, IOnBeatCollidable
    {
        private Animator _obstacleBehaviourAnimator;
        private Vector3 _playerPos;
        private BeatManager _beatManager;
        public Vector3 DefaultSpawnPosition = new Vector3(11.96f, 1.15f, 0f);

        [Inject]
        private void Construct(BeatManager beatManager)
        {
            _beatManager = beatManager;
        }

        public ObstacleBasic(Animator animator)
        {
            _obstacleBehaviourAnimator = animator;
            //_playerPos = Utils.Instance.PlayerPosition;
        }

        public override void OnScoredBehaviour()
        {
            //EventBus.Instance.InvokeObjectPassedScoreArea();
            Vector3 targetRotation = Random.insideUnitSphere;
            if (targetRotation.z < 0.05f) targetRotation *= 10f;
           // EventBus.Instance.InvokeCameraEffect(CameraEffectType.CameraRotate, Quaternion.Euler(0f, 0f, targetRotation.z));
        }

        public void CalculatePositionToCollideOnBeat(GameObject obj, float objectSpeed, ref Vector3 objectStartPosition, float defaultSpawnPositionX = 0f)
        {
            float timeUntilNextBeat = _beatManager.GetTimeToNextBeat();
            float distance = timeUntilNextBeat * objectSpeed;
            float newX = distance + DefaultSpawnPosition.x;

            objectStartPosition = new Vector3(newX, objectStartPosition.y, objectStartPosition.z);
            obj.transform.position = objectStartPosition;
        }

        public override Vector3 SetupObstacleHeight(float spriteWidth, float depth)
        {
            return Utils.GenerateNewStartPosition(spriteWidth, 1f, depth, 1f, 1.2f);
        }
    }

}
