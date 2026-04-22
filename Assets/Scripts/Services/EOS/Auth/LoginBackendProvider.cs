using Assets.Scripts.Services.Eos;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Services.EOS.Auth
{
    public interface ILoginBackendProvider
    {
        public void SetBackend(UILoginMenuEOS backend);
        public UniTask<UILoginMenuEOS> GetBackend();
    }

    /// <summary>
    /// A class for setting the current backend used for login, for example Epic Online Services
    /// </summary>
    public class LoginBackendProvider : ILoginBackendProvider
    {
        private UILoginMenuEOS _backend;
        public void SetBackend(UILoginMenuEOS backend)
        {
            _backend = backend;
            Debug.Log($"[AUTH] SetBackend{_backend}");
        }

        public async UniTask<UILoginMenuEOS> GetBackend()
        {
            Debug.Log($"[AUTH] GetBackend");
            // Wait until a valid backend is set
            while (_backend == null || !_backend)
            {
                Debug.Log($"[AUTH] GetBackend is null, waiting for setting...");
                await UniTask.Yield();
            }

            return _backend;
        }
    }
}
