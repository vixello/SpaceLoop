using Assets.Scripts.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace Assets.Scripts.Services.Cloud
{
    public class CloudSyncProviderFactory : ICloudSyncProviderFactory
    {
        //[Inject] private readonly EosCloudSyncProvider _eosCloudSyncProvider;
        private readonly Dictionary<CloudProviderType, ICloudSyncProvider> _providers;

        [Inject]
        public CloudSyncProviderFactory(IEnumerable<ICloudSyncProvider> providers)
        {
            _providers = new();
            _providers = providers.ToDictionary(p => p.ProviderType);
        }

        public ICloudSyncProvider GetCloud(CloudProviderType cloudProviderType)
        {
            if (_providers.TryGetValue(cloudProviderType, out var provider))
                return provider;
            throw new Exception($"Provider not registered: {cloudProviderType}");
        }
    }
}
