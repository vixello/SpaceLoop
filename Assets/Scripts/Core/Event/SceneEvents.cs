using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public struct SceneLoadedEvent : IEvent
    {
        public string SceneName;
        public string RuntimeKey;
    }

    public struct ScenesInitializationEvent : IEvent
    {
        public HashSet<string> ScenesToInitialize;
    }
    public struct SceneTransitionEvent : IEvent
    {
        public ISceneField[] ScenesToUnload;
        public ISceneField[] ScenesToLoad;
    }
    public struct FastSceneTransitionEvent : IEvent
    {
        public ISceneField[] ScenesToUnload;
        public ISceneField[] ScenesToLoad;
    }
}
