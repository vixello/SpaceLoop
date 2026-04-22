using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Core
{
    public class ShowLoadingScreenDisposable: IDisposable
    {
        private readonly LoadingScreen _loadingScreen;

        public ShowLoadingScreenDisposable(LoadingScreen loadingScreen)
        {
            _loadingScreen = loadingScreen;
            if (_loadingScreen)
                _loadingScreen.Show();
        }

        public void SetLoadingBarPercent(float percent)
        {
            if (_loadingScreen)
                _loadingScreen.SetBarPercent(percent);
        }
        public float GetBarPercent()
        {
            return _loadingScreen.GetBarPercent();
        }

        public void Dispose()
        {
            if (_loadingScreen) 
                _loadingScreen.Hide();
        }
    }
}