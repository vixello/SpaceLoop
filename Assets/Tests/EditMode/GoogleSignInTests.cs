using System.Collections;
using NUnit.Framework;
using Tests.Mock;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GoogleSignInTests
    {
        [Test]
        public void TestGoogleSignInSuccess()
        {
            bool callbackFired = false;

            var mockManager = new MockGoogleSignInManager(simulateSuccess: true);
            mockManager.GetGoogleIdToken((token, username) =>
            {
                callbackFired = true;
                Assert.AreEqual("mockToken12345", token);
                Assert.AreEqual("mockUser", username);
                Debug.Log($"Callback fired with token={token}, username={username}");
            });

            Assert.IsTrue(callbackFired);
        }

        [Test]
        public void TestGoogleSignInFailure()
        {
            bool callbackFired = false;

            var mockManager = new MockGoogleSignInManager(simulateSuccess: false);
            mockManager.GetGoogleIdToken((token, username) =>
            {
                callbackFired = true;
                Assert.IsNull(token);
                Assert.IsNull(username);
                Debug.Log("Callback fired with nulls (failure)");
            });

            Assert.IsTrue(callbackFired);
        }
    }
}
