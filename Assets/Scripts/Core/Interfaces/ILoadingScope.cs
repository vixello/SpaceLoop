using System;

namespace Assets.Scripts.Core
{
    public interface ILoadingScope : IDisposable
    {
        public void SetMessage(string message);
        public void Show();
        public void Hide();
    }

}