using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Achievements
{
    internal class UIAchievementPanel : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform _achievementListContainer;
        EventBinding<EnableAchievementPinningMode> _enableAchievementPinningModeBinding;
        private bool _isPinModeEnabled = false;

        public UniTask Initialize()
        {
            Debug.Log("UIAchievementPanel.Initialize called");
            _enableAchievementPinningModeBinding = new EventBinding<EnableAchievementPinningMode>(EnablePinMode);
            EventBus<EnableAchievementPinningMode>.Register(_enableAchievementPinningModeBinding);

            return UniTask.CompletedTask;   
        }

        private void Start()
        {
            EventBus<AchievementUIReady>.Raise(new AchievementUIReady
            {
                Container = _achievementListContainer
            });
        }

        private void OnDestroy()
        {
            EventBus<EnableAchievementPinningMode>.Deregister(_enableAchievementPinningModeBinding);
        }

        public void EnablePinModeButton()
        {
            EnablePinMode(new EnableAchievementPinningMode());
        }

        private async void EnablePinMode(EnableAchievementPinningMode _)
        {
            _isPinModeEnabled = !_isPinModeEnabled;
            Debug.Log("EnablePinMode");

            await WaitForContainer(Utils.ExpectedAchievementsCount);

            foreach (var achievement in _achievementListContainer.GetComponentsInChildren<IUIAchievement>())
            {
                if (achievement == null)
                    continue;

                Debug.Log("Found achievement" + achievement.Id);
                achievement.SetPinMode(_isPinModeEnabled);
            }
        }

        private async UniTask WaitForContainer(uint expectedCount, uint timeoutMs = 16000)
        {
            var start = Time.realtimeSinceStartup;

            while (_achievementListContainer.GetComponentsInChildren<IUIAchievement>().Length < expectedCount)
            {
                if ((Time.realtimeSinceStartup - start) * 1000f > timeoutMs)
                    return;

                await UniTask.Yield();
            }
        }


        public bool IsInitialized()
        {
            throw new System.NotImplementedException();
        }
    }
}
