using Assets.Scripts.Core;
using Assets.Scripts.Services.Auth;
using VContainer;

namespace Assets.Scripts.Services.EOS.Auth
{
    public class EosAuthProviderFactory : IAuthProviderFactory
    {
        private readonly GoogleAuthProvider _google;
        private readonly DeviceAuthProvider _device;

        [Inject]
        public EosAuthProviderFactory(GoogleAuthProvider google, DeviceAuthProvider device)
        {
            _google = google;
            _device = device;
        }

        public IAuthProvider GetProvider(LoginType type)
        {
            return type switch
            {
                LoginType.GoogleId => _google,
                LoginType.DeviceId => _device,
                _ => throw new System.Exception($"Unsupported login type {type}")
            };
        }
    }

}
