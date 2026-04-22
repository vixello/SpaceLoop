using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface ISceneTransitionService
    {
        UniTask RunFastTransition(SceneTransitionSO transition);
    }
}