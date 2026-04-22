using Assets.Scripts.Core;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class LoadingCircle : ILoadingScope
    {
        private Canvas _loadingCircleCanvas; 

        public LoadingCircle(Canvas loadingCircle)
        {
            _loadingCircleCanvas = loadingCircle;
            Show();
        }

        public void SetMessage(string message)
        {
            _loadingCircleCanvas.GetComponentInChildren<TextMeshProUGUI>().text = message;
        }

        public void Dispose()
        {
            Hide();
        }

        public void Show()
        {
            if(_loadingCircleCanvas != null)
            {
                _loadingCircleCanvas.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_loadingCircleCanvas != null)
                _loadingCircleCanvas.gameObject.SetActive(false);
        }
    }
}

