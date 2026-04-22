using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UI
{
    public class MenuEventSystemHandler : MonoBehaviour
    {
        [Header("References")]
        public List<Selectable> Selectables = new List<Selectable>();
        [SerializeField] protected Selectable _firstSelected;

        //[SerializeField] protected InputActionReference _navigateReference;

        [Header("Animations")]
        [SerializeField] protected float _selectedAnimationScale = 1.1f;
        [SerializeField] protected float _scaleDuration = 0.25f;
        [SerializeField] protected List<GameObject> _aniamtionExclusions = new List<GameObject>();

        protected Selectable _lastSelected;

        protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

        private void Awake()
        {
            foreach (Selectable selectable in Selectables)
            {
                AddSelectionListeners(selectable);
                _scales.Add(selectable, selectable.transform.localScale);
            }
        }

        private void OnEnable()
        {
            //_navigateReference.action.performed += OnNavigate;

            for(int i = 0; i < Selectables.Count; i++)
            {
                Selectables[i].transform.localScale = _scales[Selectables[i]];
            }

            StartCoroutine(SelectAfterDelay());
        }

        private void OnDisable()
        {
            //_navigateReference.action.performed -= OnNavigate;
        }

        protected IEnumerator SelectAfterDelay()
        {
            yield return null;
            EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
        }

        protected virtual void AddSelectionListeners(Selectable selected)
        {
            EventTrigger trigger = selected.gameObject.GetComponent<EventTrigger>();

            if(trigger == null ) 
            {
                trigger = selected.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry selectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select,
            };
            selectEntry.callback.AddListener(OnSelect);
            trigger.triggers.Add(selectEntry);

            EventTrigger.Entry deselectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Deselect,
            };
            deselectEntry.callback.AddListener(OnDeselect);
            trigger.triggers.Add(deselectEntry);


            EventTrigger.Entry pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter,
            };
            pointerEnter.callback.AddListener(OnPointerEnter);
            trigger.triggers.Add(pointerEnter);

            EventTrigger.Entry pointerExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit,
            };
            pointerExit.callback.AddListener(OnPointerExit);
            trigger.triggers.Add(pointerExit);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _lastSelected = eventData.selectedObject.GetComponent<Selectable>();

            if (_aniamtionExclusions.Contains(eventData.selectedObject)) return;

            Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
            StartCoroutine(eventData.selectedObject.transform.ScaleTo(newScale, _scaleDuration));
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
            StartCoroutine(eventData.selectedObject.transform.ScaleTo(_scales[sel], _scaleDuration));
        }

        public void OnPointerEnter(BaseEventData eventData)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;

            if(pointerEventData != null)
            {
                pointerEventData.selectedObject = pointerEventData.pointerEnter;
            }
        }

        public void OnPointerExit(BaseEventData eventData)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;

            if (pointerEventData != null)
            {
                pointerEventData.selectedObject = null;
            }
        }

        public virtual void OnNavigate(InputAction.CallbackContext context)
        {
            if(EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
            }
        }
    }
}


namespace UI
{
    public static class TransformTween
    {
        public static IEnumerator ScaleTo(this Transform transform, Vector3 targetScale, float duration, AnimationCurve curve = null)
        {
            Vector3 startScale = transform.localScale;
            float time = 0f;
            curve ??= AnimationCurve.EaseInOut(0, 0, 1, 1);

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = curve.Evaluate(time / duration);
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        public static IEnumerator PunchScale(this Transform transform, float punchAmount = 0.2f, float duration = 0.3f)
        {
            Vector3 startScale = transform.localScale;
            Vector3 maxScale = startScale * (1 + punchAmount);
            float halfDuration = duration / 2f;

            // Scale up
            yield return transform.ScaleTo(maxScale, halfDuration);
            // Scale back down
            yield return transform.ScaleTo(startScale, halfDuration);
        }
    }
}
