using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Assets.Scripts.UI.PlayerIdentity
{
    public class UITitleController : MonoBehaviour
    {
        [SerializeField] private GameObject _titleTemplate;
        [SerializeField] private Transform _titlesContainer;

        private ITitleManager _titleManager;
        private EventBinding<TitleUnlocked> _titleUnloclkedBinding;

        [Inject]
        private void Construct(ITitleManager titleManager)
        {
            _titleManager = titleManager;
        }

        private void Start()
        {
            BuildTitleUI();

            _titleUnloclkedBinding = new EventBinding<TitleUnlocked>(BuildTitleUI);
            EventBus<TitleUnlocked>.Register(_titleUnloclkedBinding);
        }

        private void OnDestroy()
        {
            EventBus<TitleUnlocked>.Deregister(_titleUnloclkedBinding);
        }

        private void BuildTitleUI()
        {
            Debug.Log("Build title ui");
            foreach (Transform child in _titlesContainer)
                Destroy(child.gameObject);

            foreach (var rt in _titleManager.Titles)
                CreateTitleUI(rt);
        }

        private void CreateTitleUI(RuntimeTitle rt)
        {
            var go = Instantiate(_titleTemplate, _titlesContainer);
            var ui = go.GetComponent<IUITitleTemplate>();

            Debug.Log($"Build title {rt.Config.TitleId} {rt.Save.IsUnlocked}");
            ui.SetId(rt.Config.TitleId);
            ui.SetDisplayName(rt.Config.DisplayName);
            ui.SetDescription(rt.Config.Description);
            ui.SetUnlockState(rt.Save.IsUnlocked);
            ui.SetSelectedState(rt.Save.IsSelected);

            ui.OnClicked += HandleTitleClicked;
        }

        private void HandleTitleClicked(string id) 
        { 
            EventBus<TitleClicked>.Raise(new TitleClicked {  Id =  id });
            RefreshSelectionUI(); 
        }

        private void RefreshSelectionUI()
        {
            foreach (Transform child in _titlesContainer)
            {
                var ui = child.GetComponent<IUITitleTemplate>();
                var rt = _titleManager.Titles.First(t => t.Config.TitleId == ui.Id);

                ui.SetSelectedState(rt.Save.IsSelected);
            }
        }
    }
}
