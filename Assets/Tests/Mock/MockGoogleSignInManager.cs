using System;
using UnityEngine;

namespace Tests.Mock
{
    public class MockGoogleSignInManager : PlayEveryWare.EpicOnlineServices.Samples.IGoogleSignInManager
    {
        private readonly bool _simulateSuccess;

        public MockGoogleSignInManager(bool simulateSuccess = true)
        {
            _simulateSuccess = simulateSuccess;
        }

        public void GetGoogleIdToken(Action<string, string> callback)
        {
            Debug.Log("MockGoogleSignInManager: GetGoogleIdToken called");

            if (_simulateSuccess)
            {
                // Simulate success after small delay
                callback?.Invoke("mockToken12345", "mockUser");
            }
            else
            {
                // Simulate failure
                callback?.Invoke(null, null);
            }
        }
    }

}
