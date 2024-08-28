namespace Huntdown
{
    public enum ItemKey
    {
        Binoculars,
        Boombox,
        CardboardBox,
        Flashlight,
        Jetpack,
        Key,
        LockPicker,
        LungApparatus,
        MapDevice,
        ProFlashlight,
        Shovel,
        StunGrenade,
        ExtensionLadder,
        TZPInhalant,
        WalkieTalkie,
        ZapGun,
        _7Ball,
        Airhorn,
        Bell,
        BigBolt,
        BottleBin,
        Brush,
        Candy,
        CashRegister,
        ChemicalJug,
        ClownHorn,
        Cog1,
        Dentures,
        DustPan,
        EggBeater,
        EnginePart1,
        FancyCup,
        FancyLamp,
        FancyPainting,
        FishTestProp,
        FlashLaserPointer,
        GoldBar,
        Hairdryer,
        MagnifyingGlass,
        MetalSheet,
        MoldPan,
        Mug,
        PerfumeBottle,
        Phone,
        PickleJar,
        PillBottle,
        Remote,
        Ring,
        RobotToy,
        RubberDuck,
        SodaCanRed,
        SteeringWheel,
        StopSign,
        TeaKettle,
        Toothpaste,
        ToyCube,
        RedLocustHive,
        RadarBooster,
        YieldSign,
        Shotgun,
        GunAmmo,
        SprayPaint,
        DiyFlashbang,
        GiftBox,
        Flask,
        TragedyMask,
        ComedyMask,
        WhoopieCushion,
        EasterEgg,
        GarbageLid,
        ToiletPaperRolls,
        Zeddog,
        WeedKillerBottle,
        ToyTrain,
        SoccerBall,
        Knife,
        ControlPad,
        PlasticCup,
    }


    public class StoredItem
    {
        private ItemKey _key;
        private string _ID;
        private string _name;
        private Item _gameItem;

        public StoredItem(ItemKey key, string id)
        {
            _key = key;
            _ID = id;
        }

        public ItemKey Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Item GameItem
        {
            get { return _gameItem; }
            set { _gameItem = value; }
        }
    }
}
