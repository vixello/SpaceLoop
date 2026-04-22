using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface ISaveSystem
    {
        public void Register(ISaveable saveable);
        public void Unregister(ISaveable saveable);
        public void RequestSave();
        public UniTask WaitUntilReady();
        public UniTask SaveLocally();
        public UniTask Load();
        public UniTask ClearLocalData();
        public UniTask SaveLoginCache(AutomaticLoginData data);
        public UniTask<AutomaticLoginData> LoadLoginCache();
    }
}
