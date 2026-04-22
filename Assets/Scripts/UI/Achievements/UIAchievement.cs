using UnityEngine.UI;
using UnityEngine;
using Assets.Scripts.Core.Interfaces;
using TMPro;
using Assets.Scripts.Core;
using VContainer;
using Assets.Scripts.Contracts;
using System.Runtime.CompilerServices;
using Assets.Scripts.Data.Rewards;


namespace Assets.Scripts.UI
{
    public class UIAchievement : MonoBehaviour, IUIAchievement
    {
        private bool _isUnlocked = false;
        public bool IsUnlocked { get => _isUnlocked; set => _isUnlocked = value; }

        private bool _isRewardClaimed = false;
        public bool IsRewardClaimed { get => _isRewardClaimed; set => _isRewardClaimed = value; }
        
        [Header("Basic Achievement Data")]
        [SerializeField] private int _index;
        public int Index { get => _index; set => _index = value; }

        private string _id;
        public string Id { get => _id; set => _id = value; }

        private bool _isPinned = false;
        private int _pinOrderValue = -1;

        [SerializeField] protected RawImage _iconImage;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _colorUnlocked;
        [SerializeField] private Color _colorLocked;

        [Header("Progress Data")]
        [SerializeField] private Slider _progress;
        [SerializeField] private TextMeshProUGUI _progressText;

        [Header("Pinned Data")]
        [SerializeField] private Image _pinImage;
        [SerializeField] private Color _pinColorUnlocked;
        [SerializeField] private Color _pinColorLocked;
        [SerializeField] private TMP_Text _pinOrder;

        [Header("Scene Transitions")]
        [SerializeField] protected SceneTransitionSO _transitionToDetailScene;

        [Header("Reward Data")]
        [SerializeField] private Image _rewardImage;
        [SerializeField] private TMP_Text _rewardAmount;
        [SerializeField] private UIRewardButton _uiRewardButton;

        private EventBinding<PinnedAchievementsChanged> _pinnedBinding;
        private EventBinding<RewardClaimed> _rewardClaimedBinding;
        protected ICommandFactory<UIAchievementClickCommand> _commandFactory;
        protected RewardVisualDatabase _rewardVisualDatabase;

        [Inject]
        private void Construct(ICommandFactory<UIAchievementClickCommand> commandFactory, RewardVisualDatabase rewardVisualDatabase)
        {
            _commandFactory = commandFactory;
            _rewardVisualDatabase = rewardVisualDatabase;
        }

        protected virtual void Start()
        {
            _pinnedBinding = new EventBinding<PinnedAchievementsChanged>(OnPinnedChanged);
            _rewardClaimedBinding = new EventBinding<RewardClaimed>(OnRewardClaimed);
            EventBus<PinnedAchievementsChanged>.Register(_pinnedBinding);
            EventBus<RewardClaimed>.Register(_rewardClaimedBinding);

            Debug.Log($"[ACHIEV] Reward button in Start = {_uiRewardButton}");
            gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            EventBus<PinnedAchievementsChanged>.Deregister(_pinnedBinding);
            EventBus<RewardClaimed>.Deregister(_rewardClaimedBinding);
        }

        protected virtual void OnPinnedChanged(PinnedAchievementsChanged changed)
        {
            if(Index>= changed.NewPinned.Length)
            {
                SetPin(false, -1);
                return;
            }

            for (int i = 0; i < changed.NewPinned.Length; ++i)
            {
                if (changed.NewPinned[i].Id == Id)
                {
                    SetPin(true, changed.NewPinned[i].PinOrder);
                    return;
                }
            }
            SetPin(false, -1);
        }

        public void Apply(UIAchievementContext ctx)
        {
            Id = ctx.Id;
            _title.text = ctx.Title;

            IsUnlocked = ctx.IsUnlocked;
            IsRewardClaimed = ctx.IsRewardClaimed;

            SetBackgroundColor(ctx.IsUnlocked);
            SetReward(ctx.Reward);

            SetProgress(ctx.Progress, ctx.MaxProgress);
            SetPin(ctx.IsPinned, ctx.PinOrder);

            _iconImage.texture = ctx.Icon;

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                GetComponent<RectTransform>());
            Debug.Log($"[ACHIEV] Reward button in Apply = {_uiRewardButton}");

            SetButtonListener(ctx.Id);

            gameObject.SetActive(true);
        }


        public void SetButtonListener(string id)
        {
            if (string.IsNullOrEmpty(id))
                 Debug.LogError("[ACHIEV] SetId called with NULL id");

            _uiRewardButton.onClick.RemoveAllListeners(); 
            _uiRewardButton.onClick.AddListener(() => _uiRewardButton.RequestRewardClaim(id));
        }

        public virtual void SetIconTexture(Texture2D texture)
        {
            _iconImage.texture = texture;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void SetNameText(string Text)
        {
            _title.text = Text;
        }

        public virtual async void OnButtonClicked()
        {
            Debug.Log($"[ACHIEV] Image is set, moving to achievement detail {Id}");

            var context = new AchievementCommandContext
            (
                achievementId: Id,
                isPinned: _isPinned,
                isAssigned: true,
                sceneTransition: _transitionToDetailScene
            );
            var command = _commandFactory.Create();
            await command.Execute(context);
        }

        public void Destroy()
        {
            if (!this) return;        
            if (!gameObject) return;  

            Destroy(gameObject);
        }

        public void SetProgress(int progress, int maxProgress)
        {
            if(maxProgress == 0)
            {
                _progress.gameObject.SetActive(false);
                _progressText.gameObject.SetActive(false);
                return;
            }

            _progress.maxValue = maxProgress;
            _progress.value = progress;
            _progressText.text = progress.ToString() + "/" + maxProgress;
        }

        public void SetBackgroundColor(bool isUnlocked)
        {
            if(isUnlocked)
            {
                _backgroundImage.color = _colorUnlocked;
            }
            else
            {
                _backgroundImage.color = _colorLocked;
            }
        }

        public void SetReward(Reward reward)
        {
            // TODO Assign correct reward iamge upon getting reward type
            // _rewardImage.sprite = reward.Type;]
            //_rewardImage.sprite = _rewardVisuals.GetIcon(reward.Type);
            _rewardImage.sprite = _rewardVisualDatabase.GetIcon(reward.Type); 
            _rewardImage.raycastTarget = IsUnlocked && !IsRewardClaimed;
            _uiRewardButton.interactable = IsUnlocked && !IsRewardClaimed;
            _rewardImage.color = IsUnlocked && IsRewardClaimed ? Color.gray : Color.white;
            _rewardAmount.text = reward.Amount.ToString();
        }

        private void OnRewardClaimed(RewardClaimed claimed)
        {
            if (!isActiveAndEnabled)
                return;

            if (_uiRewardButton == null)
            {
                Debug.Log($"[REWARD] UIAchievement '{name}' reward button = {(_uiRewardButton == null ? "NULL" : "OK")}");
                return;
            }

            if (claimed.AchievementId != Id)
                return;

            _uiRewardButton.interactable = false;
            _rewardImage.color = Color.gray;
        }


        public void SetPin(bool isPinned, int index)
        {
            _isPinned = isPinned;
            _pinImage.color = isPinned ? _pinColorUnlocked : _pinColorLocked;
            if (index == -1) return;
            _pinOrderValue = index;
            _pinOrder.text = _pinOrderValue > -1 ? (_pinOrderValue + 1).ToString() : "";
        }

        public void SetPinMode(bool pinModeEnabled)
        {
            if (!_isUnlocked) return;

            if (!_pinImage.transform.parent.gameObject.activeSelf)
            {
                _pinImage.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                _pinImage.transform.parent.gameObject.SetActive(pinModeEnabled);
            }
            _pinImage.color = _isPinned ? _pinColorUnlocked : _pinColorLocked;
            _pinOrder.text = _pinOrderValue > -1 ? (_pinOrderValue + 1).ToString() : "";
        }

        public void RequestPinToggle()
        {
            EventBus<AchievPinToggleRequested>.Raise(new AchievPinToggleRequested {
            AchievementId = Id});
        }
    }
}
