namespace Assets.Scripts.Core
{
    using UnityEngine;
    using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public interface ISceneField
    {
        AssetReference SceneReference { get; }
        string SceneName { get; }
    }

    [System.Serializable]
    public class AddressableSceneField : ISceneField
    {
        [SerializeField] private AssetReference _scene;
        public AssetReference SceneReference => _scene;

        public string SceneName
        {
            get
            {
                if (_scene == null)
                    return string.Empty;

#if UNITY_EDITOR
                return _scene.editorAsset != null
                    ? _scene.editorAsset.name
                    : _scene.RuntimeKey.ToString();
#else
    return _scene.RuntimeKey.ToString();
#endif

            }
        }
    }

    public sealed class RuntimeSceneField : ISceneField
    {
        [SerializeField] private AssetReference _scene;
        public AssetReference SceneReference => _scene;
        public string SceneName { get; }
        public RuntimeSceneField(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    [System.Serializable]
    public class SceneField : ISceneField
    {
        [SerializeField]
        private Object m_SceneAsset;

        [SerializeField] private AssetReference _scene;

        public AssetReference SceneReference => _scene;

        [SerializeField]
        private string m_SceneName = "";
        public string SceneName
        {
            get { return m_SceneName; }
        }

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, GUIContent.none, _property);
            SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
            SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
            _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
            if (sceneAsset != null)
            {
                sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

                if (sceneAsset.objectReferenceValue != null)
                {
                    sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}
