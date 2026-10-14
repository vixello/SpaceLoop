using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Core;

namespace Assets.Scripts.UI
{
    [Serializable]
    public enum TransitionAction
    {
        NavigateToScreen,
        GoBack,
        ClearHistory
    }

    [CreateAssetMenu(fileName = "NewUIScreenTransition", menuName ="UI/ScreenTransition")]
    public class UIScreenTransition : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _uxmlAsset;

        [Serializable]
        public struct ButtonTransitionEntry
        {
            public string buttonId;
            public ScreenType targetScreen;
            public TransitionAction action;
        }
        [SerializeField] private List<ButtonTransitionEntry> _buttonTransitions = new List<ButtonTransitionEntry>();

        public VisualTreeAsset UxmlAsset => _uxmlAsset;
        public List<ButtonTransitionEntry> ButtonTransitions => _buttonTransitions;
    }
}
