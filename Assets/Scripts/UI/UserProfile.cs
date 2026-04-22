using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Data.Level;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Assets.Scripts.UI
{
    public class UserProfile : MonoBehaviour, IInitializable
    {
        [Header("Basic User Data")]
        [SerializeField] TMP_Text _title;
        [SerializeField] Color _titleColor;
        [SerializeField] TMP_Text _username;
        [SerializeField] Image _profilePicture;

        [Header("User Level Data")]
        [SerializeField] TMP_Text _level;
        [SerializeField] Slider _levelProgress;
        [SerializeField] TMP_Text _levelProgressText;

        [Header("Buttons")]
        //[SerializeField] Transform _accountLinkButton;
        //[SerializeField] Transform _accountSwitchButton;

        private EventBinding<UserDataReady> _localDataReadyBinding;
        private EventBinding<TitleSelected> _titleSelectedBinding;
        private EventBinding<ProfileChanged> _profileChangedBinding;
        private LevelCurve _levelCurve;

        [Inject]
        private void Construct(LevelCurve levelCurve)
        {
            _levelCurve = levelCurve;
        }

        public UniTask Initialize()
        {
            gameObject.SetActive(false);
            _localDataReadyBinding = new EventBinding<UserDataReady>(UpdateProfile);
            _titleSelectedBinding = new EventBinding<TitleSelected>(RequestData);
            _profileChangedBinding = new EventBinding<ProfileChanged>(RequestData);
            EventBus<UserDataReady>.Register(_localDataReadyBinding);
            EventBus<TitleSelected>.Register(_titleSelectedBinding);
            EventBus<ProfileChanged>.Register(_profileChangedBinding); 

            RequestData();
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            EventBus<UserDataReady>.Deregister(_localDataReadyBinding);
            EventBus<TitleSelected>.Deregister(_titleSelectedBinding);
            EventBus<ProfileChanged>.Deregister(_profileChangedBinding);
        }

        private void RequestData()
        {
            EventBus<UserDataRequested>.Raise(new UserDataRequested { });
        }

        private void UpdateProfile(UserDataReady ready)
        {
            _username.text = ready.Username;
            _title.text = ready.Title;
            _title.fontMaterial = ready.TitleMaterial;
            _title.color = ready.TitleColor;
            _profilePicture.sprite = ready.ProfilePicture;
            _level.text = ready.Level.ToString();
            _levelProgress.value = ready.CurrentXp;
            var maxExp = _levelCurve.GetXpForLevel(ready.Level);
            _levelProgress.maxValue = maxExp;
            _levelProgressText.text = $"{ready.CurrentXp}/{maxExp}";
/*          _accountSwitchButton.gameObject.SetActive(!ready.IsGuest);
            _accountLinkButton.gameObject.SetActive(ready.IsGuest);*/
        }

        public bool IsInitialized()
        {
            throw new System.NotImplementedException();
        }
    }
}

