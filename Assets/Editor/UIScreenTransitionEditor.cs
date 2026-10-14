using Assets.Scripts.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(UIScreenTransition))]
public class UIScreenTransitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty uxmlProp = serializedObject.FindProperty("_uxmlAsset");
        EditorGUILayout.PropertyField(uxmlProp);

        UIScreenTransition transitionConfig = (UIScreenTransition)target;

        List<string> availableButtonsIds = new List<string> { "Select Button..." };
        if(transitionConfig.UxmlAsset != null)
        {
            VisualElement dummyRoot = new VisualElement();
            transitionConfig.UxmlAsset.CloneTree(dummyRoot);

            List<Button> buttons = dummyRoot.Query<Button>().ToList();
            foreach(Button button in buttons)
            {
                if (!string.IsNullOrEmpty(button.name))
                {
                    availableButtonsIds.Add(button.name);
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Button Transitions", EditorStyles.boldLabel);

        // Draw the list of entries
        SerializedProperty transitionsProp = serializedObject.FindProperty("_buttonTransitions");

        for(int i = 0; i < transitionsProp.arraySize; i++)
        {
            SerializedProperty entryProp = transitionsProp.GetArrayElementAtIndex(i);
            SerializedProperty buttonIdProp = entryProp.FindPropertyRelative("buttonId");
            SerializedProperty targetScreenProp = entryProp.FindPropertyRelative("targetScreen");
            SerializedProperty buttonAction = entryProp.FindPropertyRelative("action");

            EditorGUILayout.BeginVertical("box");

            // Dropdown for Button ID parsed from source UXML
            string currentId = buttonIdProp.stringValue;
            int currentIndex = availableButtonsIds.IndexOf(currentId);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup($"Button [{i}]", currentIndex, availableButtonsIds.ToArray());
            if(newIndex > 0 && newIndex < availableButtonsIds.Count)
            {
                buttonIdProp.stringValue = availableButtonsIds[newIndex];
            }
            else if (newIndex == 0)
            {
                buttonIdProp.stringValue = string.Empty;
            }

            // Object field for the target VisualTreeAsset
            EditorGUILayout.PropertyField(targetScreenProp, new GUIContent("Target Screen"));
            EditorGUILayout.PropertyField(buttonAction, new GUIContent("Action"));

            // Remove button
            if (GUILayout.Button("Remove Transition", GUILayout.Width(130)))
            {
                transitionsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if(GUILayout.Button("Add Button Transition"))
        {
            transitionsProp.InsertArrayElementAtIndex(transitionsProp.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
