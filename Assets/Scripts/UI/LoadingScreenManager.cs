using Core;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Core;

namespace Assets.Scripts.UI
{
    public class LoadingScreenManager: ILoadingScreenManager/*IDisposable,*/
    {
        private readonly LoadingScreen _loadingScreen;
        private readonly IUIScreenManager _uiScreenManager;
        private VisualElement _fillBar;
        private Label _percentLabel;
        private float _currentPercent;

        public LoadingScreenManager(IUIScreenManager uIScreenManager)
        {
/*            _loadingScreen = loadingScreen;
            if (_loadingScreen)
                _loadingScreen.Show();
*/
            _uiScreenManager = uIScreenManager;
        }

        public void Show(ScreenType loadingScreenType = ScreenType.Bootstrap)
        {
            _uiScreenManager?.ShowLoadingOverlay(loadingScreenType);
            CacheUIElements();
        }

        public void Hide()
        {
            _uiScreenManager?.HideLoadingOverlay();
            _fillBar = null;
            _percentLabel = null;
        }

        private void CacheUIElements()
        {
            UIDocument doc = _uiScreenManager.LoadingUIDocument;

            if(doc != null && doc.rootVisualElement != null)
            {
                VisualElement root = doc.rootVisualElement;

                _fillBar = root.Q<VisualElement>(className: "boot__fill");
                _percentLabel = root.Q<Label>(className: "boot__pct");
            }
        }

        public void SetLoadingBarPercent(float percent)
        {
            /*            if (_loadingScreen)
                            _loadingScreen.SetBarPercent(percent);*/

            _currentPercent = Mathf.Clamp01(percent);

            float percentageValue = _currentPercent * 100f;

            if (_fillBar == null || _percentLabel == null)
            {
                CacheUIElements();
            }

            if (_fillBar != null)
            {
                _fillBar.style.width = Length.Percent(percentageValue);
            }

            if (_percentLabel != null)
            {
                int intPercent = Mathf.RoundToInt(percentageValue);
                _percentLabel.text = $"DOWNLOADING ASSETS  {intPercent}%";
            }
        }
        public float GetBarPercent()
        {
            // return _loadingScreen.GetBarPercent();
            return _currentPercent;
        }

        public void Dispose()
        {
/*            if (_loadingScreen) 
                _loadingScreen.Hide();*/
        }
    }
}