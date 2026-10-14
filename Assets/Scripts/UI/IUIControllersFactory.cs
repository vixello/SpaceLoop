
using Assets.Scripts.Core;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    public interface IUIControllersFactory
    {
        public bool TryGetController(ScreenType screenType, out UIController controller);
    }


    public abstract class UIController
    {
        protected VisualElement _root;
        public VisualElement Root => _root;
        public virtual void Initialize(UIDocument document)
        {
            if (document == null) return;
            _root = document.rootVisualElement;

        }
        public virtual void Tick() { }
        public virtual void OnShow() { }
        public virtual void OnHide() { }
    }
}
