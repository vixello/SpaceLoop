using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Services.Auth;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Tests.Assets.Tests.Mock;
using Tests.Mock;
using UnityEngine;

namespace Tests.Assets.Tests.EditMode
{
    public class AuthService_LoginTests
    {
        [Test]
        public async Task Login_Canceled_DisposesLoadingScope()
        {
            //Arange 
            var loading = new MockLoadingScope();
            Func<ILoadingScope> beginLoading = () => loading;

            var loginResult = new LoginResult
            {
                Status = AuthResult.Canceled
            };

            var mockProvider = new MockAuthProvider(loginResult);
            var providerFactory = new MockAuthProviderFactory(mockProvider);

            var dialog = new MockDialogService();
            var identity = new MockIdentityManager(new Data.IdentityData
            {
                IsGuest = true,
                AccountTransfered = false,

                Level = 20,
                CurrentXp = 102,

                CurrentTitleId = "NEWCOMER",

                EosPuid = null,
                Username = "testdude",
            });
            var saveSystem = new MockSaveSystem();

            var authService = new AuthService(
               dialog,
               providerFactory,
               identity,
               saveSystem,
               beginLoading,
               ScriptableObject.CreateInstance<SceneTransitionSO>()
           );

            // Act
            await authService.Login(LoginType.GoogleId).AsTask();
            Debug.Log($"[TEST] authService handed back login. {loading.Disposed}");
            Debug.Log("[TEST] Test loading instance: " + loading.GetHashCode());

            // Assert
            Assert.IsTrue(loading.Disposed, "Loading must be disposed on canceled login");
        }

        [Test]
        public async Task Login_Failed_DisposesLoadingScope()
        {
            //Arange 
            var loading = new MockLoadingScope();
            Func<ILoadingScope> beginLoading = () => loading;

            var loginResult = new LoginResult
            {
                Status = AuthResult.Failed,
                ErrorMessage = "Something went wrong",
                EosResult = Epic.OnlineServices.Result.InvalidAuth
            };

            var mockProvider = new MockAuthProvider(loginResult);
            var providerFactory = new MockAuthProviderFactory(mockProvider);

            var dialog = new MockDialogService();
            var identity = new MockIdentityManager(new Data.IdentityData
            {
                IsGuest = true,
                AccountTransfered = false,

                Level = 20,
                CurrentXp = 102,

                CurrentTitleId = "NEWCOMER",

                EosPuid = null,
                Username = "testdude",
            });
            var saveSystem = new MockSaveSystem();

            var authService = new AuthService(
               dialog,
               providerFactory,
               identity,
               saveSystem,
               beginLoading,
               ScriptableObject.CreateInstance<SceneTransitionSO>()
           );

            // Act
            await authService.Login(LoginType.GoogleId).AsTask();
            Debug.Log($"[TEST] authService handed back login. {loading.Disposed}");
            Debug.Log("[TEST] Test loading instance: " + loading.GetHashCode());

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Something went wrong InvalidAuth");
            // Assert
            Assert.IsTrue(loading.Disposed, "Loading must be disposed on failed login");
        }

        [Test]
        public async Task Login_Succeeded_DisposesLoadingScope()
        {
            //Arange 
            var loading = new MockLoadingScope();
            Func<ILoadingScope> beginLoading = () => loading;

            var loginResult = new LoginResult
            {
                Status = AuthResult.Success
            };

            var mockProvider = new MockAuthProvider(loginResult);
            var providerFactory = new MockAuthProviderFactory(mockProvider);

            var dialog = new MockDialogService();
            var identity = new MockIdentityManager(new Data.IdentityData
            {
                IsGuest = true,
                AccountTransfered = false,

                Level = 20,
                CurrentXp = 102,

                CurrentTitleId = "NEWCOMER",

                EosPuid = null,
                Username = "testdude",
            });
            var saveSystem = new MockSaveSystem();

            var authService = new AuthService(
               dialog,
               providerFactory,
               identity,
               saveSystem,
               beginLoading,
               ScriptableObject.CreateInstance<SceneTransitionSO>()
           );

            // Act
            await authService.Login(LoginType.GoogleId).AsTask();
            Debug.Log($"[TEST] authService handed back login. {loading.Disposed}");
            Debug.Log("[TEST] Test loading instance: " + loading.GetHashCode());

            // Assert
            Assert.IsTrue(loading.Disposed, "Loading must be disposed on succeeded login");
        }
    }
}
