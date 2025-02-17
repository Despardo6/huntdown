using BepInEx;
using HarmonyLib;
using Huntdown.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using static Huntdown.ConfigSettings;


namespace Huntdown
{
    [BepInPlugin(_modGUID, _modName, _modVersion)]
    [BepInDependency("evaisa.lethalthings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("LethalCompanyHarpGhost", BepInDependency.DependencyFlags.SoftDependency)]
    // Why does Aloe has its own spawning logic???
    //[BepInDependency("com.github.biodiversitylc.Biodiversity", BepInDependency.DependencyFlags.SoftDependency)]
    // Cant seem to make balls spawnable in reward pools :(
    //[BepInDependency("LethalMon", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Jordo.NeedyCats", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Theronguard.EmergencyDice", BepInDependency.DependencyFlags.SoftDependency)]
    public class Huntdown : BaseUnityPlugin
    {
        private const string _modGUID = "doggosuki.Huntdown";
        private const string _modName = "Huntdown";
        private const string _modVersion = "1.6.0";

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

        public static bool lethalThingsPresent = false;
        public static bool hauntedHarpistPresent = false;
        //public static bool bioDiversityPresent = false;
        public static bool needyCatsPresent = false;
        //public static bool lethalMonPresent = false;
        public static bool emergencyDicePresent = false;

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

            StoredItem[] nonNullItems = items.Where(item => item != null).ToArray();

            return new RewardPool
            (
                nonNullItems,
                (int)ConfigEntries[(int)index].BoxedValue
            );
        }

        private Mission CreateMission(string name, ConfigIndexes weightIndex, Dictionary<EnemyKey, int> enemies, ConfigIndexes toggleIndex, RewardPool rewardPool)
        {
            // Read enabled status from config.
            var enabled = (bool)ConfigEntries[(int)toggleIndex].BoxedValue;
            var weight = (int)ConfigEntries[(int)weightIndex].BoxedValue;

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

            lethalThingsPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("evaisa.lethalthings");
            if (lethalThingsPresent)
            {
                _logger.LogInfo("LethalThings detected.  Zombie missions will be available.");
            }

            hauntedHarpistPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("LethalCompanyHarpGhost");
            if (hauntedHarpistPresent)
            {
                _logger.LogInfo("Haunted Harpist detected.  Ghost missions will be available.");
            }

            /*bioDiversityPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.github.biodiversitylc.Biodiversity");
            if (bioDiversityPresent)
            {
                _logger.LogInfo("Biodiversity detected.  The Aloe mission will be available.");
            }*/

            /*lethalMonPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("LethalMon");
            if (lethalMonPresent)
            {
                _logger.LogInfo("LethalMon detected.  Pokeball rewards will be available.");
            }*/

            needyCatsPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Jordo.NeedyCats");
            if (needyCatsPresent)
            {
                _logger.LogInfo("Needy Cats detected.  Cat rewards will be available.");
            }

            emergencyDicePresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Theronguard.EmergencyDice");
            if (emergencyDicePresent)
            {
                _logger.LogInfo("Emergency Dice detected.  Dice rewards will be available.");
            }

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
                    new StoredEnemy(EnemyKey.ForestKeeper, "ForestGiant (EnemyType)"),
                    new StoredEnemy(EnemyKey.Zombie, "Maggie (EnemyType)"),
                    new StoredEnemy(EnemyKey.HauntedHarpist, "HarpGhost (EnemyType)"),
                    new StoredEnemy(EnemyKey.PhantomPiper, "BagpipesGhost (EnemyType)"),
                    new StoredEnemy(EnemyKey.EnforcerGhost, "EnforcerGhost (EnemyType)"),
                    //new StoredEnemy(EnemyKey.Aloe, "AloeEnemyType (EnemyType)"),
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
                    new StoredItem(ItemKey.BeltBag, "BeltBag (Item)"),
                    new StoredItem(ItemKey.CaveDwellerBaby, "CaveDwellerBaby (Item)"),
                    new StoredItem(ItemKey.Pokeball, "Pokeball (Item)"),
                    new StoredItem(ItemKey.GreatBall, "GreatBall (Item)"),
                    new StoredItem(ItemKey.UltraBall, "UltraBall (Item)"),
                    new StoredItem(ItemKey.MasterBall, "MasterBall (Item)"),
                    new StoredItem(ItemKey.CatItem, "CatItem (Item)"),
                    new StoredItem(ItemKey.Pouch, "Pouch (Item)"),
                    new StoredItem(ItemKey.ArsonPlush, "ArsonPlush (Item)"),
                    new StoredItem(ItemKey.ArsonPlushDirty, "ArsonPlushDirty (Item)"),
                    new StoredItem(ItemKey.CookieFumo, "CookieFumo (Item)"),
                    new StoredItem(ItemKey.ToimariPlush, "ToimariPlush (Item)"),
                    new StoredItem(ItemKey.glizzy, "glizzy (Item)"),
                    new StoredItem(ItemKey.GhostPlushieItemData, "GhostPlushieItemData (Item)"),
                    new StoredItem(ItemKey.HamisPlush, "HamisPlush (Item)"),
                    new StoredItem(ItemKey.GnarpyPlush, "GnarpyPlush (Item)"),
                    new StoredItem(ItemKey.RocketLauncher, "RocketLauncher (Item)"),
                    new StoredItem(ItemKey.GremlinEnergy, "GremlinEnergy (Item)"),
                    new StoredItem(ItemKey.Dingus, "Dingus (Item)"),
                    new StoredItem(ItemKey.ToyGun, "ToyGun (Item)"),
                    new StoredItem(ItemKey.Chronos, "Chronos (Item)"),
                    new StoredItem(ItemKey.GamblerItem, "GamblerItem (Item)"),
                    new StoredItem(ItemKey.Saint, "Saint (Item)"),
                    new StoredItem(ItemKey.Sacrificer, "Sacrificer (Item)"),
                    new StoredItem(ItemKey.SurfacedDieItem, "SurfacedDieItem (Item)"),
                    new StoredItem(ItemKey.Rusty, "Rusty (Item)"),
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
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.SprayPaint] : null,
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.TZPInhalant] : null,
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.StunGrenade] : null,
                            _storedItems[(int)ItemKey.EasterEgg],
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.BeltBag] : null,
                            _storedItems[(int)ItemKey.DustPan],
                            _storedItems[(int)ItemKey.Brush],
                            //lethalMonPresent ? _storedItems[(int)ItemKey.Pokeball] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.glizzy] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.ToyGun] : null,
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
                            //lethalMonPresent ? _storedItems[(int)ItemKey.GreatBall] : null,
                            hauntedHarpistPresent ? _storedItems[(int)ItemKey.GhostPlushieItemData] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.HamisPlush] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.GremlinEnergy] : null,
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue && lethalThingsPresent
                                ? _storedItems[(int)ItemKey.RocketLauncher]
    :                           null,
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue && lethalThingsPresent
                                ? _storedItems[(int)ItemKey.Pouch]
    :                           null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.ToimariPlush] : null,
                            emergencyDicePresent ? _storedItems[(int)ItemKey.SurfacedDieItem] : null,
                            emergencyDicePresent ? _storedItems[(int)ItemKey.Sacrificer] : null,
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
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.Jetpack] : null,
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.ZapGun] : null,
                            _storedItems[(int)ItemKey.ToyTrain],
                            _storedItems[(int)ItemKey.SoccerBall],
                            _storedItems[(int)ItemKey.ControlPad],
                            _storedItems[(int)ItemKey.ToiletPaperRolls],
                            //lethalMonPresent ? _storedItems[(int)ItemKey.UltraBall] : null,
                            needyCatsPresent ? _storedItems[(int)ItemKey.CatItem] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.ArsonPlush] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.ArsonPlushDirty] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.CookieFumo] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.GnarpyPlush] : null,
                            emergencyDicePresent ? _storedItems[(int)ItemKey.Chronos] : null,
                            emergencyDicePresent ? _storedItems[(int)ItemKey.Rusty] : null,
                        },
                        ConfigIndexes.RewardHigh
                    ),

                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.GoldBar],
                            _storedItems[(int)ItemKey.CashRegister],
                            (bool)ConfigEntries[(int)ConfigIndexes.EnableToolRewards].BoxedValue ? _storedItems[(int)ItemKey.MapDevice] : null,
                            _storedItems[(int)ItemKey.Zeddog],
                            _storedItems[(int)ItemKey.PlasticCup],
                            //lethalMonPresent ? _storedItems[(int)ItemKey.MasterBall] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.Dingus] : null,
                            emergencyDicePresent ? _storedItems[(int)ItemKey.Saint] : null,
                            needyCatsPresent ? _storedItems[(int)ItemKey.CatItem] : null,
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
                    ),

                    CreateRewardPool
                    (
                        new StoredItem[]
                        {
                            _storedItems[(int)ItemKey.Zeddog],
                            _storedItems[(int)ItemKey.PlasticCup],
                            //lethalMonPresent ? _storedItems[(int)ItemKey.MasterBall] : null,
                            lethalThingsPresent ? _storedItems[(int)ItemKey.Dingus] : null,
                            emergencyDicePresent ? _storedItems[(int)ItemKey.Saint] : null,
                            needyCatsPresent ? _storedItems[(int)ItemKey.CatItem] : null,
                        },
                        ConfigIndexes.RewardBrutal
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
                List<Mission> missionsList = new List<Mission>()
                {
                    CreateMission(
                        "Snare Flea",
                        ConfigIndexes.WeightFlea,
                        CreateEnemyDictionary((EnemyKey.SnareFlea, 1)),
                        ConfigIndexes.ToggleFlea,
                        _possibleRewardPools[(int)RewardPoolKey.SmallRewardPool]
                    ),

                    CreateMission(
                        "Bunker Spider",
                        ConfigIndexes.WeightSpider,
                        CreateEnemyDictionary((EnemyKey.BunkerSpider, 1)),
                        ConfigIndexes.ToggleSpider,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    // Poor hoarding bug...
                    CreateMission(
                        "Hoarding Bug",
                        ConfigIndexes.WeightHoarder,
                        CreateEnemyDictionary((EnemyKey.HoarderBug, 1)),
                        ConfigIndexes.ToggleHoarder,
                        _possibleRewardPools[(int)RewardPoolKey.SmallRewardPool]
                    ),

                    CreateMission(
                        "Bracken",
                        ConfigIndexes.WeightBracken,
                        CreateEnemyDictionary((EnemyKey.Bracken, 1)),
                        ConfigIndexes.ToggleBracken,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission(
                        "Thumper",
                        ConfigIndexes.WeightThumper,
                        CreateEnemyDictionary((EnemyKey.Thumper, 1)),
                        ConfigIndexes.ToggleThumper,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission(
                        "Nutcracker",
                        ConfigIndexes.WeightNutcracker,
                        CreateEnemyDictionary((EnemyKey.Nutcracker, 1)),
                        ConfigIndexes.ToggleNutcracker,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission(
                        "Masked",
                        ConfigIndexes.WeightMasked,
                        CreateEnemyDictionary((EnemyKey.Masked, 1)),
                        ConfigIndexes.ToggleMasked,
                        _possibleRewardPools[(int)RewardPoolKey.MaskRewardPool]
                    ),

                    CreateMission(
                        "A Good Boy",
                        ConfigIndexes.WeightDog,
                        CreateEnemyDictionary((EnemyKey.EyelessDog, 1)),
                        ConfigIndexes.ToggleDog,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ),

                    CreateMission(
                        "Bug Mafia",
                        ConfigIndexes.WeightMafia,
                        CreateEnemyDictionary((EnemyKey.HoarderBug, 5)),
                        ConfigIndexes.ToggleMafia,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission(
                        "Blunderbug",
                        ConfigIndexes.WeightBlunderbug,
                        CreateEnemyDictionary((EnemyKey.HoarderBug, 1)),
                        ConfigIndexes.ToggleBlunderbug,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission(
                        "Infestation",
                        ConfigIndexes.WeightInfestation,
                        CreateEnemyDictionary(
                            (EnemyKey.HoarderBug, 2),
                            (EnemyKey.SnareFlea, 2),
                            (EnemyKey.BunkerSpider, 1)
                        ),
                        ConfigIndexes.ToggleInfestation,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission(
                        "Last Month's Interns",
                        ConfigIndexes.WeightLastcrew,
                        CreateEnemyDictionary(
                            (EnemyKey.Masked, 4)
                        ),
                        ConfigIndexes.ToggleLastcrew,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ),

                    CreateMission(
                        "Butler",
                        ConfigIndexes.WeightButler,
                        CreateEnemyDictionary(
                            (EnemyKey.Butler, 1)
                        ),
                        ConfigIndexes.ToggleButler,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ),

                    CreateMission(
                        "Maneater",
                        ConfigIndexes.WeightManeater,
                        CreateEnemyDictionary(
                            (EnemyKey.Maneater, 1)
                        ),
                        ConfigIndexes.ToggleManeater,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ),

                   CreateMission(
                        "Stabbin' Bros",
                        ConfigIndexes.WeightStabbinBros,
                        CreateEnemyDictionary(
                            (EnemyKey.HoarderBug, 3),
                            (EnemyKey.Butler, 2)
                        ),
                        ConfigIndexes.ToggleStabbinBros,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                   CreateMission(
                        "Giant Size: Upgraded",
                        ConfigIndexes.WeightGiantSize,
                        CreateEnemyDictionary(
                            (EnemyKey.Random, 1)
                        ),
                        ConfigIndexes.ToggleGiantSize,
                        _possibleRewardPools[(int)RewardPoolKey.BrutalRewardPool]
                    ),

                   CreateMission(
                        "Big Trouble Little Enemies",
                        ConfigIndexes.WeightLittleEnemies,
                        CreateEnemyDictionary(
                            (EnemyKey.Random, 15)
                        ),
                        ConfigIndexes.ToggleLittleEnemies,
                        _possibleRewardPools[(int)RewardPoolKey.BrutalRewardPool]
                    ),

                   CreateMission(
                        "Who let the puppies out?",
                        ConfigIndexes.WeightPuppies,
                        CreateEnemyDictionary(
                            (EnemyKey.EyelessDog, 12)
                        ),
                        ConfigIndexes.TogglePuppies,
                        _possibleRewardPools[(int)RewardPoolKey.BrutalRewardPool]
                    ),

                    CreateMission(
                        "Baboon Gang",
                        ConfigIndexes.WeightBaboonGang,
                        CreateEnemyDictionary(
                            (EnemyKey.BaboonHawk, 3)
                        ),
                        ConfigIndexes.ToggleBaboonGang,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),

                    CreateMission(
                        "Facility Keeper",
                        ConfigIndexes.WeightFacilityKeeper,
                        CreateEnemyDictionary(
                            (EnemyKey.ForestKeeper, 1)
                        ),
                        ConfigIndexes.ToggleFacilityKeeper,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ),
                };

                if (lethalThingsPresent)
                {
                    missionsList.Add(CreateMission(
                        "Zombie",
                        ConfigIndexes.WeightZombie,
                        CreateEnemyDictionary((EnemyKey.Zombie, 1)),
                        ConfigIndexes.ToggleZombie,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ));

                    missionsList.Add(CreateMission(
                        "Last Year's Interns",
                        ConfigIndexes.WeightZombieCrew,
                        CreateEnemyDictionary((EnemyKey.Zombie, 4)),
                        ConfigIndexes.ToggleZombieCrew,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ));

                    missionsList.Add(CreateMission(
                        "Zombie Apocalypse",
                        ConfigIndexes.WeightZombieApocalypse,
                        CreateEnemyDictionary((EnemyKey.Zombie, 15)),
                        ConfigIndexes.ToggleZombieApocalypse,
                        _possibleRewardPools[(int)RewardPoolKey.BrutalRewardPool]
                    ));
                }

                if (hauntedHarpistPresent)
                {
                    missionsList.Add(CreateMission(
                        "Haunted Harpist",
                        ConfigIndexes.WeightHauntedHarpist,
                        CreateEnemyDictionary((EnemyKey.HauntedHarpist, 1)),
                        ConfigIndexes.ToggleHauntedHarpist,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ));

                    missionsList.Add(CreateMission(
                        "Phantom Piper",
                        ConfigIndexes.WeightPhantomPiper,
                        CreateEnemyDictionary((EnemyKey.PhantomPiper, 1)),
                        ConfigIndexes.TogglePhantomPiper,
                        _possibleRewardPools[(int)RewardPoolKey.LargeRewardPool]
                    ));

                    missionsList.Add(CreateMission(
                        "Enforcer",
                        ConfigIndexes.WeightEnforcerGhost,
                        CreateEnemyDictionary((EnemyKey.EnforcerGhost, 1)),
                        ConfigIndexes.ToggleEnforcerGhost,
                        _possibleRewardPools[(int)RewardPoolKey.MediumRewardPool]
                    ));

                    missionsList.Add(CreateMission(
                        "The Firing Squad",
                        ConfigIndexes.WeightFiringSquad,
                        CreateEnemyDictionary((EnemyKey.EnforcerGhost, 4), (EnemyKey.Nutcracker, 1)),
                        ConfigIndexes.ToggleFiringSquad,
                        _possibleRewardPools[(int)RewardPoolKey.HugeRewardPool]
                    ));
                }

                _possibleMissions = missionsList.ToArray();
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