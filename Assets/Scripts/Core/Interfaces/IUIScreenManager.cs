using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IUIScreenManager
    {
        UIDocument MainUIDocument { get; }
        UIDocument LoadingUIDocument { get; }
        void ClearHistory();
        void GoBackInHistory();
        void SwitchScreen(ScreenType targetScreenType, bool isGoingBack = false, bool clearHistory = false);
        void ShowLoadingOverlay(ScreenType loadingScreenType);
        void HideLoadingOverlay();
    }
}
