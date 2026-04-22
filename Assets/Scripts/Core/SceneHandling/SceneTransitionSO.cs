using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = "SceneTransition", menuName = "ScriptableObjects/SceneTransition", order = 1)]
    public class SceneTransitionSO : ScriptableObject
    {
        [Header("Scenes To Load")]
        public AddressableSceneField[] ScenesToLoad;
        [Header("Scenes To Unload")]
        public AddressableSceneField[] ScenesToUnload;
    }
}
