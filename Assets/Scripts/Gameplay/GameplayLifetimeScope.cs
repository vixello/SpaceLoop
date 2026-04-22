using Assets.Scripts.Core;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Assets.Scripts.Gameplay
{
    internal class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<SceneInitializer>();
            builder.RegisterComponentInHierarchy<SpaceCraftController>();

            // DELETE LATER
            builder.RegisterComponentInHierarchy<InputManager>();
            builder.RegisterComponentInHierarchy<UpdatePublisher>();
            builder.Register<PlayerSpaceshipManager>(Lifetime.Singleton);
        }
    }
}
