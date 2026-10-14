using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface ILoadingScreenManager
    {
        void Show(ScreenType screenType);
        void Hide();
        void SetLoadingBarPercent(float percent);
    }
}
