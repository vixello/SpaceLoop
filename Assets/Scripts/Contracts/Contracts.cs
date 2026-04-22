
namespace Assets.Scripts.Contracts 
{
    public enum RewardType
    {
        CoinTierOne,
        CoinTierTwo,
        CoinTierThree,
        Title,
        Item, // for example building asset 
        Xp
    }

    [System.Serializable]
    public struct Reward
    {
        public bool IsValid;
        public int Amount;
        public int XpAmount;
        public RewardType Type;
    }

}


