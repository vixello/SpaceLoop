using Assets.Scripts.Core.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Obstacles
{
    public class ObstacleBehaviourFactory
    {
        private static ObstacleBehaviourFactory _instance;

        public static ObstacleBehaviourFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ObstacleBehaviourFactory();
                return _instance;
            }
        }

        public virtual ObstacleBehaviour CreateObstacleBehaviour(ObstacleType obstacleType, Animator animator)
        {
            ObstacleBehaviour obstacle = null;

            switch (obstacleType)
            {
                case ObstacleType.Basic:
                    obstacle = new ObstacleBasic(animator);
                    break;
            }

            return obstacle;
        }
    }
}
