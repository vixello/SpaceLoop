namespace Assets.Scripts.Core
{
    public struct UsernameAccepted : IEvent
    {
        public string Username;
    }

    public struct PlayerXpGained : IEvent
    {
        public int Amount;
        public PlayerXpGained(int amount) => Amount = amount;
    }

    public struct PlayerLevelUp : IEvent
    {
        public int NewLevel;
        public PlayerLevelUp(int lvl) => NewLevel = lvl;
    }

    public struct PlayerTitleChanged : IEvent
    {
        public string TitleId;
        public PlayerTitleChanged(string id) => TitleId = id;
    }

    public struct TitleSelected : IEvent
    {
        public string Id; 
    }
    public struct TitleUnlocked : IEvent
    {
        public string Id;
    }
    public struct TitleClicked : IEvent
    {
        public string Id;
    }
    public struct TitleUnlockRequested : IEvent
    {
        public string Id;
    }

    public struct PFPClicked : IEvent
    {
        public string Id;
    }

    public struct ProfileChanged : IEvent
    {
        public string Id;
    }
}
