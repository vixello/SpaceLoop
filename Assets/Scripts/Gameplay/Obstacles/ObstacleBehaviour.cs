namespace Assets.Scripts.Gameplay.Obstacles
{
    using Assets.Scripts.Core;
    using UnityEngine;

    public abstract class ObstacleBehaviour
    {
        public virtual void OnPlayerCollisionBehavior()
        {
            //GameStateManager.Instance.ChangeGameState(GameState.GameOver);
        }

        public virtual void OnScoredBehaviour() { }
        public virtual void OnSpawnBehaviour(GameObject gameObject) { }
        public virtual void OnEnableBehaviour() { }
        public virtual void OnDeactivateBehaviour() { }
        public virtual void OnMelodyBehaviour(GameObject gameObject, float obstacleSpeed) { }
        //public virtual void SetNewPositionToCollideOnBeat(float timeToReachBeat, float objectSpeed, float defaultSpawnPositionX, ref Vector3 objectStartPosition) { }
        /*    public virtual void CalculatePositionToCollideOnBeat(GameObject obj, float objectSpeed, ref Vector3 objectStartPosition, float defaultSpawnPositionX = 0f) 
            {
                float timeUntilNextBeat = BeatManager.Instance.GetTimeToNextBeat();
                float distance = timeUntilNextBeat * objectSpeed;
                float newX = distance + SharedProperties.DefaultSpawnPosition.x;

                objectStartPosition = new Vector3(newX, objectStartPosition.y, objectStartPosition.z);
                obj.transform.position = objectStartPosition;
            }*/
        public abstract Vector3 SetupObstacleHeight(float spriteWidth, float depth);
    }

}
