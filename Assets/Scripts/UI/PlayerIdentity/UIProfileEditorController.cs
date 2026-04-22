using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data;
using Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Assets.Scripts.UI.PlayerIdentity
{
    public class UIProfileEditorController : MonoBehaviour
    {
        [SerializeField] private GameObject _profilePictureTemplate;
        [SerializeField] private Transform _profilePictureContainer;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private Image _saveButton;
        [SerializeField] private SceneTransitionSO _transitionAfterSave;

        private string _selectedId;
        private IProfileEditorManager _profileEditorManager;
        private IUsernameValidationSystem _usernameValidationSystem;
        private EventBinding<PFPClicked> _pfpClickedBinding;
        private ISceneTransitionService _sceneTransitionService;

        [Inject]
        private void Construct(IProfileEditorManager profileEditorManager, 
            IUsernameValidationSystem usernameValidationSystem, ISceneTransitionService sceneTransitionService)
        {
            _profileEditorManager = profileEditorManager;
            _usernameValidationSystem = usernameValidationSystem;
            _sceneTransitionService = sceneTransitionService;
        }

        private void Start()
        {
            BuildUI();
            _pfpClickedBinding = new EventBinding<PFPClicked>(OnPFPSelected);
            EventBus<PFPClicked>.Register(_pfpClickedBinding);
        }

        private void OnDestroy()
        {
            EventBus<PFPClicked>.Deregister(_pfpClickedBinding);
        }

        private void FixedUpdate()
        {
            if(_profileEditorManager.GetSavedUsername() != _usernameInput.text)
            {
                CheckUsernameInput();
            }
        }

        private void BuildUI()
        {
            Debug.Log("Build pfp ui");
            foreach (Transform child in _profilePictureContainer)
                Destroy(child.gameObject);

            foreach (var rt in _profileEditorManager.GetDatabaseData())
                CreateProfileUI(rt);
        }

        private void CreateProfileUI(ProfilePictureConfig rt)
        {
            var go = Instantiate(_profilePictureTemplate, _profilePictureContainer);
            var ui = go.GetComponent<IUIPictureTemplate>();
            var savedPictures = _profileEditorManager.GetPictureSavedData();
            _selectedId = savedPictures.SelectedPictureId?? "picture_01";

            ui.SetId(rt.Id);
            ui.SetIamgeData(rt.Image, rt.LevelToUnlock);
            ui.SetUnlockStatus(savedPictures.UnlockedPictureIds.Contains(rt.Id));
            ui.SetSelectedStatus(rt.Id == _selectedId);
        }

        private void RefreshSelectionUI()
        {
            foreach (Transform child in _profilePictureContainer)
            {
                var ui = child.GetComponent<IUIPictureTemplate>();
                ui.SetSelectedStatus(ui.Id == _selectedId);
            }
        }

        public void OnSaveClicked()
        {
            _profileEditorManager.ProfileChanged(_selectedId, _usernameInput.text.Trim());
            _sceneTransitionService.RunFastTransition(_transitionAfterSave);
        }

        public void OnCancelClicked()
        {
            _sceneTransitionService.RunFastTransition(_transitionAfterSave);
        }

        private void CheckUsernameInput()
        {
            string usernameText = _usernameInput.text.Trim();
            var result = _usernameValidationSystem.Validate(usernameText);

            if (result.ErrorType != UsernameErrorType.Allowed)
            {
                ShowError(result.Message);
                SetSaveButtonVisibility(true, Color.gray);
                return;
            }

            // Username OK
            _errorText.text = "";

            SetSaveButtonVisibility(true, Color.white);
            Debug.Log($"Username valid: {usernameText}");
            _errorText.text = "";
        }

        private void SetSaveButtonVisibility(bool raycast, Color color)
        {
            _saveButton.raycastTarget = raycast;
            _saveButton.color = color;
        }

        private void ShowError(string message)
        {
            _errorText.text = message;
            //_errorText.localizedString = new LocalizedString("UsernameErrors", result.Message);
        }

        private void OnPFPSelected(PFPClicked e)
        {
            _selectedId = e.Id;
            RefreshSelectionUI();
        }
    }
}
