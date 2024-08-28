using BepInEx;
using HarmonyLib;
using Huntdown.Patches;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using static Huntdown.ConfigSettings;


namespace Huntdown
{
    [BepInPlugin(_modGUID, _modName, _modVersion)]
    public class Huntdown : BaseUnityPlugin
    {
        private const string _modGUID = "doggosuki.Huntdown";
        private const string _modName = "Huntdown";
        private const string _modVersion = "1.4.1";

        private readonly Harmony _harmony = new Harmony(_modGUID);
        public static Huntdown _instance;
        public static BepInEx.Logging.ManualLogSource _logger;

        //public static bool bHunted = true; // Has the target been hunted yet
        //public static bool bMissionInitialised = false;
        public static bool _isHost = false; // Is the user the host
        public static bool _objectsStored = false; // Has the mod already stored items and enemies
        public static int _framesPassedSinceInput = 0;

        //public static MissionData CurrentMission; // The current mission assigned to the team
        public static SelectableLevel _currentLevel; // The current level
        public static EnemyVent[] _allEnemyVents; // Array of vents on level
        public static int[] _allScrapValue; // Array of ints for the value of each item of scrap in the level
        public static NetworkObjectReference[] _allScrapNetwork; // Array of networked references to each item of scrap in the level
        
        public static StoredEnemy[] _storedEnemies;
        public static StoredItem[] _storedItems;
        public static RewardPool[] _possibleRewardPools;
        public static Mission[] _possibleMissions;
        
        public static TerminalNode _terminalNode = new TerminalNode();
        public static TerminalKeyword _terminalKeyword = new TerminalKeyword();
        public static Mission _currentMission;
        public static EnemyAI _lastEnemyKilled;

        Dictionary<EnemyKey, int> CreateEnemyDictionary(params (EnemyKey key, int value)[] keyValuePairs)
        {
            var dictionary = new Dictionary<EnemyKey, int>();
            foreach (var pair in keyValuePairs)
            {
                dictionary.Add(pair.key, pair.value);
            }
            return dictionary;
        }

        private RewardPool CreateRewardPool(StoredItem[] items, ConfigIndexes index)
        {
            var val = (int)ConfigEntries[(int)index].BoxedValue;

            _logger.LogInfo($"Creating reward pool. Index: {index}, Value: {val}");

            return new RewardPool
            (
                items,
                (int)ConfigEntries[(int)index].BoxedValue
            );
        }

        private Mission CreateMission(string name, ConfigIndexes weightIndex, Dictionary<EnemyKey, int> enemies, ConfigIndexes toggleIndex, RewardPool rewardPool)
        {
            var weight = (int)ConfigEntries[(int)weightIndex].BoxedValue;
            var enabled = (bool)ConfigEntries[(int)toggleIndex].BoxedValue;

            _logger.LogInfo($"Creating mission: {name}, Weight: {weight}, Enabled: {enabled}");

            return new Mission(name, weight, enemies, enabled, rewardPool);
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            _logger = this.Logger;

            try
            {
                _logger.LogInfo("Binding configs.");
                BindConfigSettings();
                _logger.LogInfo("Configs successfully bound.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not bind configs.\n" + ex.Message);
            }

            try
            {
                _logger.LogInfo("Assigning keys to enemies.");
                _storedEnemies = new StoredEnemy[]
                {
                    new StoredEnemy(EnemyKey.SnareFlea, "Centipede (EnemyType)"),
                    new StoredEnemy(EnemyKey.BunkerSpider, "SandSpider (EnemyType)"),
                    new StoredEnemy(EnemyKey.HoarderBug, "HoarderBug (EnemyType)"),
                    new StoredEnemy(EnemyKey.Bracken, "Flowerman (EnemyType)"),
                    new StoredEnemy(EnemyKey.Thumper, "Crawler (EnemyType)"),
                    new StoredEnemy(EnemyKey.Nutcracker, "Nutcracker (EnemyType)"),
                    new StoredEnemy(EnemyKey.Masked, "MaskedPlayerEnemy (EnemyType)"),
                    new StoredEnemy(EnemyKey.EyelessDog, "MouthDog (EnemyType)"),
                    new StoredEnemy(EnemyKey.Butler, "Butler (EnemyType)"),
					new StoredEnemy(EnemyKey.Maneater, "CaveDweller (EnemyType)"),
                    new StoredEnemy(EnemyKey.BaboonHawk, "BaboonHawk (EnemyType)"),
                };
                _logger.LogInfo("Keys successfully assigned to enemies.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not assign keys to enemies.\n" + ex.Message);
            }

            try
            {
                _logger.LogInfo("Assigning keys to items.");
                _storedItems = new StoredItem[]
                {
                    new StoredItem(ItemKey.Binoculars, "Binoculars (Item)"),
                    new StoredItem(ItemKey.Boombox, "Boombox (Item)"),
                    new StoredItem(ItemKey.CardboardBox, "CardboardBox (Item)"),
                    new StoredItem(ItemKey.Flashlight, "Flashlight (Item)"),
                    new StoredItem(ItemKey.Jetpack, "Jetpack (Item)"),
                    new StoredItem(ItemKey.Key, "Key (Item)"),
                    new StoredItem(ItemKey.LockPicker, "LockPicker (Item)"),
                    new StoredItem(ItemKey.LungApparatus, "LungApparatus (Item)"),
                    new StoredItem(ItemKey.MapDevice, "MapDevice (Item)"),
                    new StoredItem(ItemKey.ProFlashlight, "ProFlashlight (Item)"),
                    new StoredItem(ItemKey.Shovel, "Shovel (Item)"),
                    new StoredItem(ItemKey.StunGrenade, "StunGrenade (Item)"),
                    new StoredItem(ItemKey.ExtensionLadder, "ExtensionLadder (Item)"),
                    new StoredItem(ItemKey.TZPInhalant, "TZPInhalant (Item)"),
                    new StoredItem(ItemKey.WalkieTalkie, "WalkieTalkie (Item)"),
                    new StoredItem(ItemKey.ZapGun, "ZapGun (Item)"),
                    new StoredItem(ItemKey._7Ball, "7Ball (Item)"),
                    new StoredItem(ItemKey.Airhorn, "Airhorn (Item)"),
                    new StoredItem(ItemKey.Bell, "Bell (Item)"),
                    new StoredItem(ItemKey.BigBolt, "BigBolt (Item)"),
                    new StoredItem(ItemKey.BottleBin, "BottleBin (Item)"),
                    new StoredItem(ItemKey.Brush, "Brush (Item)"),
                    new StoredItem(ItemKey.Candy, "Candy (Item)"),
                    new StoredItem(ItemKey.CashRegister, "CashRegister (Item)"),
                    new StoredItem(ItemKey.ChemicalJug, "ChemicalJug (Item)"),
                    new StoredItem(ItemKey.ClownHorn, "ClownHorn (Item)"),
                    new StoredItem(ItemKey.Cog1, "Cog1 (Item)"),
                    new StoredItem(ItemKey.Dentures, "Dentures (Item)"),
                    new StoredItem(ItemKey.DustPan, "DustPan (Item)"),
                    new StoredItem(ItemKey.EggBeater, "EggBeater (Item)"),
                    new StoredItem(ItemKey.EnginePart1, "EnginePart1 (Item)"),
                    new StoredItem(ItemKey.FancyCup, "FancyCup (Item)"),
                    new StoredItem(ItemKey.FancyLamp, "FancyLamp (Item)"),
                    new StoredItem(ItemKey.FancyPainting, "FancyPainting (Item)"),
                    new StoredItem(ItemKey.FishTestProp, "FishTestProp (Item)"),
                    new StoredItem(ItemKey.FlashLaserPointer, "FlashLaserPointer (Item)"),
                    new StoredItem(ItemKey.GoldBar, "GoldBar (Item)"),
                    new StoredItem(ItemKey.Hairdryer, "Hairdryer (Item)"),
                    new StoredItem(ItemKey.MagnifyingGlass, "MagnifyingGlass (Item)"),
                    new StoredItem(ItemKey.MetalSheet, "MetalSheet (Item)"),
                    new StoredItem(ItemKey.MoldPan, "MoldPan (Item)"),
                    new StoredItem(ItemKey.Mug, "Mug (Item)"),
                    new StoredItem(ItemKey.PerfumeBottle, "PerfumeBottle (Item)"),
                    new StoredItem(ItemKey.Phone, "Phone (Item)"),
                    new StoredItem(ItemKey.PickleJar, "PickleJar (Item)"),
                    new StoredItem(ItemKey.PillBottle, "PillBottle (Item)"),
                    new StoredItem(ItemKey.Remote, "Remote (Item)"),
                    new StoredItem(ItemKey.Ring, "Ring (Item)"),
                    new StoredItem(ItemKey.RobotToy, "RobotToy (Item)"),
                    new StoredItem(ItemKey.RubberDuck, "RubberDuck (Item)"),
                    new StoredItem(ItemKey.SodaCanRed, "SodaCanRed (Item)"),
                    new StoredItem(ItemKey.SteeringWheel, "SteeringWheel (Item)"),
                    new StoredItem(ItemKey.StopSign, "StopSign (Item)"),
                    new StoredItem(ItemKey.TeaKettle, "TeaKettle (Item)"),
                    new StoredItem(ItemKey.Toothpaste, "Toothpaste (Item)"),
                    new StoredItem(ItemKey.ToyCube, "ToyCube (Item)"),
                    new StoredItem(ItemKey.RedLocustHive, "RedLocustHive (Item)"),
                    new StoredItem(ItemKey.RadarBooster, "RadarBooster (Item)"),
                    new StoredItem(ItemKey.YieldSign, "YieldSign (Item)"),
                    new StoredItem(ItemKey.Shotgun, "Shotgun (Item)"),
                    new StoredItem(ItemKey.GunAmmo, "GunAmmo (Item)"),
                    new StoredItem(ItemKey.SprayPaint, "SprayPaint (Item)"),
                    new StoredItem(ItemKey.DiyFlashbang, "DiyFlashbang (Item)"),
                    new StoredItem(ItemKey.GiftBox, "GiftBox (Item)"),
                    new StoredItem(ItemKey.Flask, "Flask (Item)"),
                    new StoredItem(ItemKey.TragedyMask, "TragedyMask (Item)"),
                    new StoredItem(ItemKey.ComedyMask, "ComedyMask (Item)"),
                    new StoredItem(ItemKey.WhoopieCushion, "WhoopieCushion (Item)"),
                    new StoredItem(ItemKey.EasterEgg, "EasterEgg (Item)"),
                    new StoredItem(ItemKey.GarbageLid, "GarbageLid (Item)"),
                    new StoredItem(ItemKey.ToiletPaperRolls, "ToiletPaperRolls (Item)"),
                    new StoredItem(ItemKey.Zeddog, "Zeddog (Item)"),
                    new StoredItem(ItemKey.WeedKillerBottle, "WeedKillerBottle (Item)"),
                    new StoredItem(ItemKey.ToyTrain, "ToyTrain (Item)"),
                    new StoredItem(ItemKey.SoccerBall, "SoccerBall (Item)"),
                    new StoredItem(ItemKey.Knife, "Knife (Item)"),
                    new StoredItem(ItemKey.ControlPad, "ControlPad (Item)"),
                    new StoredItem(ItemKey.PlasticCup, "PlasticCup (Item)"),
                };
                _logger.LogInfo("Keys successfully assigned to items.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not assign keys to items.\n" + ex.Message);
            }

            try
            {
                _logger.LogInfo("Creating reward pools for missions.");
                _possibleRewardPools = new RewardPool[]
                {
                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.RubberDuck],
                            _storedItems[(int)ItemKey.ToyCube],
                            _storedItems[(int)ItemKey.Candy],
                            _storedItems[(int)ItemKey.WhoopieCushion],
                            _storedItems[(int)ItemKey.FishTestProp],
                            _storedItems[(int)ItemKey.SprayPaint],
                            _storedItems[(int)ItemKey.TZPInhalant],
                            _storedItems[(int)ItemKey.StunGrenade],
                            _storedItems[(int)ItemKey.EasterEgg],
                            _storedItems[(int)ItemKey.WeedKillerBottle],
                        },
                        ConfigIndexes.RewardLow
                    ),

                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.PickleJar],
                            _storedItems[(int)ItemKey.Phone],
                            _storedItems[(int)ItemKey.Remote],
                            _storedItems[(int)ItemKey.Hairdryer],
                            _storedItems[(int)ItemKey.RobotToy],
                            _storedItems[(int)ItemKey.MagnifyingGlass],
                            _storedItems[(int)ItemKey.Dentures],
                            _storedItems[(int)ItemKey.GarbageLid],
                        },
                        ConfigIndexes.RewardMedium
                    ),

                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.Ring],
                            _storedItems[(int)ItemKey.FancyPainting],
                            _storedItems[(int)ItemKey.PerfumeBottle],
                            _storedItems[(int)ItemKey.FancyCup],
                            _storedItems[(int)ItemKey.FancyLamp],
                            _storedItems[(int)ItemKey.Jetpack],
                            _storedItems[(int)ItemKey.ZapGun],
                            _storedItems[(int)ItemKey.Knife],
                            _storedItems[(int)ItemKey.ToyTrain],
                            _storedItems[(int)ItemKey.SoccerBall],
                            _storedItems[(int)ItemKey.ControlPad],
                            _storedItems[(int)ItemKey.ToiletPaperRolls],
                        },
                        ConfigIndexes.RewardHigh
                    ),

                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.GoldBar],
                            _storedItems[(int)ItemKey.CashRegister],
                            _storedItems[(int)ItemKey.MapDevice],
                            _storedItems[(int)ItemKey.Zeddog],
                            _storedItems[(int)ItemKey.PlasticCup],
                        },
                        ConfigIndexes.RewardExtreme
                    ),

                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.TragedyMask],
                            _storedItems[(int)ItemKey.ComedyMask],
                        },
                        ConfigIndexes.RewardMedium
                    )
                };
                _logger.LogInfo("Reward pools successfully created.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not create reward pools.\n" + ex.Message);
            }

            try
            {
                _logger.LogInfo("Creating missions with information from player configuration.");
                _possibleMissions = new Mission[]
                {
                    CreateMission
                    (
                        "Snare Flea",
                        ConfigIndexes.WeightFlea,
                        CreateEnemyDictionary((EnemyKey.SnareFlea, 1)),
                        ConfigIndexes.ToggleFlea,
                        _possibleRewardPools[(int)RewardPoolKey.SmallRewardPool]
                    ),

                    CreateMission
                    (
                        "Bunker Spider",
                        ConfigIndexes.WeightSpider,
                        CreateEnemyDictionary((EnemyKey.BunkerSpider, 1)),
                        ConfigIndexes.ToggleSpider,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission
                    (
                        "Hoarding Bug",
                        ConfigIndexes.WeightHoarder,
                        CreateEnemyDictionary((EnemyKey.HoarderBug, 1)),
                        ConfigIndexes.ToggleHoarder,
                        _possibleRewardPools[(int)RewardPoolKey.SmallRewardPool]
                    ),

                    CreateMission
                    (
                        "Bracken",
                        ConfigIndexes.WeightBracken,
                        CreateEnemyDictionary((EnemyKey.Bracken, 1)),
                        ConfigIndexes.ToggleBracken,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission
                    (
                        "Thumper",
                        ConfigIndexes.WeightThumper,
                        CreateEnemyDictionary((EnemyKey.Thumper, 1)),
                        ConfigIndexes.ToggleThumper,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission
                    (
                        "Nutcracker",
                        ConfigIndexes.WeightNutcracker,
                        CreateEnemyDictionary((EnemyKey.Nutcracker, 1)),
                        ConfigIndexes.ToggleNutcracker,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission
                    (
                        "Masked",
                        ConfigIndexes.WeightMasked,
                        CreateEnemyDictionary((EnemyKey.Masked, 1)),
                        ConfigIndexes.ToggleMasked,
                        _possibleRewardPools[(int)RewardPoolKey.MaskRewardPool]
                    ),

                    CreateMission
                    (
                        "A Good Boy",
                        ConfigIndexes.WeightDog,
                        CreateEnemyDictionary((EnemyKey.EyelessDog, 1)),
                        ConfigIndexes.ToggleDog,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ),

                    CreateMission
                    (
                        "Bug Mafia",
                        ConfigIndexes.WeightMafia,
                        CreateEnemyDictionary((EnemyKey.HoarderBug, 5)),
                        ConfigIndexes.ToggleMafia,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission
                    (
                        "Blunderbug",
                        ConfigIndexes.WeightBlunderbug,
                        CreateEnemyDictionary((EnemyKey.HoarderBug, 1)),
                        ConfigIndexes.ToggleBlunderbug,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission
                    (
                        "Infestation",
                        ConfigIndexes.WeightInfestation,
                        CreateEnemyDictionary
                        (
                            (EnemyKey.HoarderBug, 2),
                            (EnemyKey.SnareFlea, 2),
                            (EnemyKey.BunkerSpider, 1)
                        ),
                        ConfigIndexes.ToggleInfestation,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission
                    (
                        "Last Month's Interns",
                        ConfigIndexes.WeightLastcrew,
                        CreateEnemyDictionary
                        (
                            (EnemyKey.Masked, 4)
                        ),
                        ConfigIndexes.ToggleLastcrew,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ),

                    CreateMission
                    (
                        "Butler",
                        ConfigIndexes.WeightButler,
                        CreateEnemyDictionary
                        (
                            (EnemyKey.Butler, 1)
                        ),
                        ConfigIndexes.ToggleButler,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

					CreateMission
					(
						"Maneater",
						ConfigIndexes.WeightManeater,
						CreateEnemyDictionary
						(
							(EnemyKey.Maneater, 1)
						),
						ConfigIndexes.ToggleManeater,
						_possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
					),

                    CreateMission
                    (
                        "Baboon Gang",
                        ConfigIndexes.WeightBaboonGang,
                        CreateEnemyDictionary
                        (
                            (EnemyKey.BaboonHawk, 3)
                        ),
                        ConfigIndexes.ToggleBaboonGang,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                };
                _logger.LogInfo("Missions successfully created.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not create missions.\n" + ex.Message);
            }

            try
            {
                _logger.LogInfo("Patching mod.");
                _harmony.PatchAll(typeof(Huntdown));
                _harmony.PatchAll(typeof(StartOfRoundPatch));
                _harmony.PatchAll(typeof(RoundManagerPatch));
                _harmony.PatchAll(typeof(TerminalPatch));
                _harmony.PatchAll(typeof(MouthDogAIPatch));
                _harmony.PatchAll(typeof(HoarderBugAIPatch));

#if DEBUG
                _logger.LogWarning("Debug mode is active.");
                _harmony.PatchAll(typeof(PlayerControllerBPatch));
                _harmony.PatchAll(typeof(ApplicationPatch));
#endif
                _logger.LogInfo("Patches were successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not patch mod.\n" + ex.Message);
            }

            _logger.LogInfo("Huntdown mod loaded.");
        }
    }
}
