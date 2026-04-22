using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using PlayEveryWare.EpicOnlineServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Services.EOS.Cloud
{
    public class StatServiceMiddleman : IInitializable
    {
        private EventBinding<StatIncreaseRequested> _statIcnreaseBinding;

        
        public async void IncrementStat(StatIncreaseRequested e)
        {
            await StatsService.Instance.IngestStatAsync(e.Name, e.Amount);
        }

        public UniTask Initialize()
        {
            _statIcnreaseBinding = new EventBinding<StatIncreaseRequested>(IncrementStat);
            EventBus<StatIncreaseRequested>.Register(_statIcnreaseBinding);
            return UniTask.CompletedTask;
        }

        public bool IsInitialized()
        {
            throw new NotImplementedException();
        }
    }
}
