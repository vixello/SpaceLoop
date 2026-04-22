using Assets.Scripts.Core.Interfaces;
using Data;

namespace Tests.Assets.Tests.Mock
{
    public class MockIdentityManager : IIdentityManager
    {
        private IdentityData _identityData;
        public bool UsernameSet { get; set; }

        public MockIdentityManager(IdentityData identityData)
        {
            _identityData = identityData;
        }

        public void ClearIdentity()
        {
            _identityData = new IdentityData();
        }

        public IdentityData GetIdentity()
        {
            return _identityData;
        }

        public bool IsUsernameSet()
        {
            return UsernameSet;
        }
    }

}
