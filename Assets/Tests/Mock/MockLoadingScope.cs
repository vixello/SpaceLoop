using Assets.Scripts.Core;
using UnityEngine;

namespace Tests.Assets.Tests.Mock
{
    public class MockLoadingScope : ILoadingScope
    {
        public bool Disposed { get; private set; }
        public string Message { get; private set; }

        public MockLoadingScope() { Debug.Log("[TEST] MockLoadingScope created " + GetHashCode()); }
       
        public void SetMessage(string message)
        {
            Message = message;
        }

        public void Dispose()
        {
            Debug.Log("[TEST] MockLoadingScope disposed " + GetHashCode());
            Disposed = true;
        }

        public void Show()
        {
            Debug.Log("Dialog Show");
        }

        public void Hide()
        {
            Debug.Log("Dialog Hide");
        }
    }
}
