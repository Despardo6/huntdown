using System.Collections.Generic;

namespace Huntdown
{
    public enum RewardPoolKey
    {
        SmallRewardPool,
        MediumRewardPool,
        LargeRewardPool,
        HugeRewardPool,
        MaskRewardPool
    }

    public class RewardPool
    {
        private List<StoredItem> _rewards;
        private int _value;

        public RewardPool(StoredItem[] rewards, int value)
        {
            _rewards = new List<StoredItem>(rewards);
            _value = value;
        }

        public List<StoredItem> Rewards
        {
            get { return _rewards; }
            set { _rewards = value; }
        }

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}

   /*public class RewardPool
    {
        //public SpawnableItemWithRarity[] SpawnableItems;
        //public Item ItemType;
        //public string[] ItemIDs;
        public Dictionary<string, Item> _itemsToSpawn;
        public int _itemValue;

        public RewardPool(Dictionary<string, Item> itemsToSpawn, int value)
        {
            _itemsToSpawn = itemsToSpawn;
            _itemValue = value;
        }

        /*public static readonly RewardData[] PossibleRewards = new RewardData[]
        {
            new RewardData
            {
                SpawnableItems = new SpawnableItemWithRarity[5],
                ItemIDs = new string[]
                {
                    "RubberDuck (Item)",
                    "ToyCube (Item)",
                    "Candy (Item)",
                    "WhoopieCushion (Item)",
                    "FishTestProp (Item)"
                },
                ItemValue = 1//ConfigSettings.lowDiffultyReward.Value,
            },

            new RewardData
            {
                SpawnableItems = new SpawnableItemWithRarity[8],
                ItemIDs = new string[]
                {
                    "PickleJar (Item)",
                    "Phone (Item)",
                    "FlashLaserPointer (Item)",
                    "Remote (Item)",
                    "Hairdryer (Item)",
                    "RobotToy (Item)",
                    "MagnifyingGlass (Item)",
                    "Dentures (Item)"
                },
                ItemValue = 1//ConfigSettings.medDiffultyReward.Value,
            },

            new RewardData
            {
                SpawnableItems = new SpawnableItemWithRarity[5],
                ItemIDs = new string[]
                {
                    "Ring (Item)",
                    "FancyPainting (Item)",
                    "PerfumeBottle (Item)",
                    "FancyCup (Item)",
                    "FancyLamp (Item)"
                },
                ItemValue = 1//ConfigSettings.highDiffultyReward.Value,
            },

            new RewardData
            {
                //RewardKey = 3,
                SpawnableItems = new SpawnableItemWithRarity[2],
                ItemIDs = new string[]
                {
                    "CashRegister (Item)",
                    "GoldBar (Item)",
                },
                ItemValue = 1//ConfigSettings.extremeDiffultyReward.Value,
            },

            new RewardData
            {
                SpawnableItems = new SpawnableItemWithRarity[2],
                ItemIDs = new string[]
                {
                    "TragedyMask (Item)",
                    "ComedyMask (Item)"
                },
                ItemValue = 1//ConfigSettings.medDiffultyReward.Value,
            }
        };
        */
    //}
//}

        