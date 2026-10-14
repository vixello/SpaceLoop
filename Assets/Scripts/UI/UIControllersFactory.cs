using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using VContainer;

namespace Assets.Scripts.UI
{
    public class UIControllersFactory : IUIControllersFactory
    {
        private readonly IObjectResolver _resolver;

        private readonly Dictionary<ScreenType, Type> _controllerRegistry = new Dictionary<ScreenType, Type>()
        {
            { ScreenType.MainMenu, typeof(MainMenuController) },
            { ScreenType.Login, typeof(LoginController) },
            //{ ScreenType.Shop, typeof(ShopController) },
        };

        [Inject]
        public UIControllersFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public bool TryGetController(ScreenType screenType, out UIController controller)
        {
            controller = null;
            if(_controllerRegistry.TryGetValue(screenType, out Type controllerType))
            {
                controller = (UIController)_resolver.Resolve(controllerType);
                return true;
            }
            return false;
        }
    }
}
