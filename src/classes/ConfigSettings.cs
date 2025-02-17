using BepInEx.Configuration;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Huntdown
{
    public enum GeneralConfigKey
    {
        DisplayTarget,
        GeneralPercentageChance,
        ToggleWeightDynamic,
        ChangeSubtext
    }

    public enum RewardConfigKey
    {
        RewardLow,
        RewardMedium,
        RewardHigh,
        RewardExtreme
    }

    public static class ConfigSettings
    {
        private static string GetDescription(this Enum value)
        {
            FieldInfo info = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = info.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }
            return value.ToString();
        }

        public struct ConfigurableSetting
        {
            public Enum Index;
            public string Section;
            public string Key;
            public object DefaultValue;
            public ConfigDescription Description;
        }

        private enum ConfigSections
        {
            [Description("1: General Settings")]
            General,
            [Description("2: Toggle Missions")]
            Toggle,
            [Description("3: Mission Weights")]
            Weight,
            [Description("4: Mission Rewards")]
            Reward
        }

        public enum DisplayTargetSettings
        {
            Full,
            Some,
            None
        }

        public enum ConfigIndexes
        {
            DisplayTarget,
            GeneralPercentageChance,
            ToggleWeightDynamic,
            ChangeSubtext,
            EnableToolRewards,

            ///////////////////////////////////////////////////////

            ToggleFlea,
            ToggleSpider,
            ToggleHoarder,
            ToggleBracken,
            ToggleThumper,
            ToggleNutcracker,
            ToggleMasked,
            ToggleDog,
            ToggleMafia,
            ToggleBlunderbug,
            ToggleInfestation,
            ToggleLastcrew,
            ToggleButler,
			ToggleManeater,
            ToggleStabbinBros,
            ToggleGiantSize,
            ToggleLittleEnemies,
            TogglePuppies,
            ToggleBaboonGang,
            ToggleFacilityKeeper,
            ToggleZombie,
            ToggleZombieCrew,
            ToggleZombieApocalypse,
            ToggleHauntedHarpist,
            TogglePhantomPiper,
            ToggleEnforcerGhost,
            ToggleFiringSquad,

            ///////////////////////////////////////////////////////

            WeightFlea,
            WeightSpider,
            WeightHoarder,
            WeightBracken,
            WeightThumper,
            WeightNutcracker,
            WeightMasked,
            WeightDog,
            WeightMafia,
            WeightBlunderbug,
            WeightInfestation,
            WeightLastcrew,
            WeightButler,
			WeightManeater,
            WeightStabbinBros,
            WeightGiantSize,
            WeightLittleEnemies,
            WeightPuppies,
            WeightBaboonGang,
            WeightFacilityKeeper,
            WeightZombie,
            WeightZombieCrew,
            WeightZombieApocalypse,
            WeightHauntedHarpist,
            WeightPhantomPiper,
            WeightEnforcerGhost,
            WeightFiringSquad,

            ///////////////////////////////////////////////////////

            RewardLow,
            RewardMedium,
            RewardHigh,
            RewardExtreme,
            RewardBrutal
        }

        public static readonly ConfigurableSetting[] AllConfigurableSettings = new ConfigurableSetting[]
        {
            new ConfigurableSetting
            {
                Index = ConfigIndexes.DisplayTarget,
                Section = ConfigSections.General.GetDescription(),
                Key = "Display Target at Round Start",
                DefaultValue = DisplayTargetSettings.Full,
                Description = new ConfigDescription("0: Display both whether you have a target, and its name.\n1: Display whether you have a target, but without its name.\n2: Never display if you have a target.", new AcceptableValueRange<int>(0, Enum.GetNames(typeof(DisplayTargetSettings)).Length - 1)),
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.GeneralPercentageChance,
                Section = ConfigSections.General.GetDescription(),
                Key = "Percentage Chance of Mission",
                DefaultValue = 100,
                Description = new ConfigDescription("The percent chance that your team will receive a target to hunt.", new AcceptableValueRange<int>(0, 100))
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleWeightDynamic,
                Section = ConfigSections.General.GetDescription(),
                Key = "Dynamic Weighting System",
                DefaultValue = true,
                Description = new ConfigDescription("Makes it more likely to get missions you haven't been given yet to keep things fresh.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ChangeSubtext,
                Section = ConfigSections.General.GetDescription(),
                Key = "Change ScanNode Subtext",
                DefaultValue = false,
                Description = new ConfigDescription("If true, changes the ScanNode's subText instead of headerText for 'TARGET' label.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.EnableToolRewards,
                Section = ConfigSections.General.GetDescription(),
                Key = "Enable Tool Rewards",
                DefaultValue = true,
                Description = new ConfigDescription("If true, tool items will be included in reward pools.")
            },

            ///////////////////////////////////////////////////////

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleFlea,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Snare Flea Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Snare Flea can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleSpider,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Bunker Spider Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Bunker Spider can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleHoarder,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Hoarder Bug Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Hoarding Bug can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleBracken,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Bracken Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Bracken can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleThumper,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Thumper Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Thumper can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleNutcracker,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Nutcracker Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Nutcracker can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleMasked,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Masked Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Masked can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleDog,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Good Boy Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether a Good Boy can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleMafia,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Bug Mafia Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Bug Mafia can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleBlunderbug,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Blunderbug Mission Enabled",
                DefaultValue = false,
                Description = new ConfigDescription("(WIP, recommended to keep disabled) Whether the Blunderbug can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleInfestation,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Infestation Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Infestation (2 hoarding bugs, 2 snare fleas and 1 bunker spider) can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleLastcrew,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Last Months Interns Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Last Month's Interns (4 masked) can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleButler,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Butler Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Butler can be assigned as the hunt target or not.")
            },

			new ConfigurableSetting
			{
				Index = ConfigIndexes.ToggleManeater,
				Section = ConfigSections.Toggle.GetDescription(),
				Key = "Maneater Mission Enabled",
				DefaultValue = true,
				Description = new ConfigDescription("Whether the Maneater can be assigned as the hunt target or not.")
			},

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleStabbinBros,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Stabbin Bros Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Stabbin' Bros can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleGiantSize,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Giant Size Upgraded Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Giant Size: Upgraded can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleLittleEnemies,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Big Trouble Little Enemies Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Big Trouble Little Enemies can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.TogglePuppies,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Who let the puppies out Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("Whether the Who let the puppies out? (12 tiny dogs) can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleBaboonGang,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Baboon Gang Mission Enabled",
                DefaultValue = false,
                Description = new ConfigDescription("Whether the Baboon Gang (3 baboon hawks) can be assigned as the hunt target or not. (Disabled by default due to clients needing StarlancerAIFix for outside enemies to work inside.)")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleFacilityKeeper,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Facility Keeper Mission Enabled",
                DefaultValue = false,
                Description = new ConfigDescription("Whether the Facility Keeper can be assigned as the hunt target or not. (Disabled by default due to clients needing StarlancerAIFix for outside enemies to work inside.)")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleZombie,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Zombie Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if LethalThings mod is present.) Whether the Zombie can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleZombieCrew,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Last Years Interns Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if LethalThings mod is present.) Whether the Last Year's Interns (4 Zombies) can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleZombieApocalypse,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Zombie Apocalypse Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if LethalThings mod is present.) Whether the Zombie Apocalypse (15 Zombies) can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleHauntedHarpist,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Haunted Harpist Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Whether the Haunted Harpist can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.TogglePhantomPiper,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Phantom Piper Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Whether the Phantom Piper can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleEnforcerGhost,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "Ethereal Enforcer Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Whether the Ethereal Enforcer can be assigned as the hunt target or not.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.ToggleFiringSquad,
                Section = ConfigSections.Toggle.GetDescription(),
                Key = "The Firing Squad Mission Enabled",
                DefaultValue = true,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Whether the The Firing Squad (4 Ethereal Enforcers and 1 Nutcracker) can be assigned as the hunt target or not.")
            },

            ///////////////////////////////////////////////////////

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightFlea,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Snare Flea Mission Weight",
                DefaultValue = 100,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Snare Flea will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightSpider,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Bunker Spider Mission Weight",
                DefaultValue = 100,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Bunker Spider will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightHoarder,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Hoarder Bug Mission Weight",
                DefaultValue = 100,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Hoarder Bug will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightBracken,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Bracken Mission Weight",
                DefaultValue = 50,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Bracken will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightThumper,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Thumper Mission Weight",
                DefaultValue = 100,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Thumper will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightNutcracker,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Nutcracker Mission Weight",
                DefaultValue = 50,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Nutcracker will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightMasked,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Masked Mission Weight",
                DefaultValue = 75,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Masked will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightDog,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Good Boy Mission Weight",
                DefaultValue = 20,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that a Good Boy will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightMafia,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Bug Mafia Mission Weight",
                DefaultValue = 50,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Bug Mafia will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightBlunderbug,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Blunderbug Mission Weight",
                DefaultValue = 0,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Blunderbug will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightInfestation,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Infestation Mission Weight",
                DefaultValue = 25,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Infestation will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightLastcrew,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Last Months Interns Mission Weight",
                DefaultValue = 15,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that Last Month's Interns will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightButler,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Butler Mission Weight",
                DefaultValue = 50,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Butler will be the target.")
            },

			new ConfigurableSetting
			{
				Index = ConfigIndexes.WeightManeater,
				Section = ConfigSections.Weight.GetDescription(),
				Key = "Maneater Mission Weight",
				DefaultValue = 10,
				Description = new ConfigDescription("Higher value = more likely. The likelihood that the Maneater will be the target.")
			},

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightStabbinBros,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Stabbin Bros Mission Weight",
                DefaultValue = 40,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Stabbin' Bros will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightGiantSize,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Giant Size Upgraded Mission Weight",
                DefaultValue = 20,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Giant Size: Upgraded will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightLittleEnemies,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Big Trouble Little Enemies Mission Weight",
                DefaultValue = 20,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Big Trouble Little Enemies will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightPuppies,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Who let the puppies out Mission Weight",
                DefaultValue = 20,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Who let the puppies out? will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightBaboonGang,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Baboon Gang Mission Weight",
                DefaultValue = 40,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Baboon Gang will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightFacilityKeeper,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Facility Keeper Mission Weight",
                DefaultValue = 25,
                Description = new ConfigDescription("Higher value = more likely. The likelihood that the Facility Keeper will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightZombie,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Zombie Mission Weight",
                DefaultValue = 75,
                Description = new ConfigDescription("(Will be target only if LethalThings mod is present.) Higher value = more likely. The likelihood that the Zombie will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightZombieCrew,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Last Years Interns Mission Weight",
                DefaultValue = 15,
                Description = new ConfigDescription("(Will be target only if LethalThings mod is present.) Higher value = more likely. The likelihood that the Last Year's Interns will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightZombieApocalypse,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Zombie Apocalypse Mission Weight",
                DefaultValue = 8,
                Description = new ConfigDescription("(Will be target only if LethalThings mod is present.) Higher value = more likely. The likelihood that the Zombie Apocalypse will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightHauntedHarpist,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Haunted Harpist Mission Weight",
                DefaultValue = 30,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Higher value = more likely. The likelihood that the Haunted Harpist will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightPhantomPiper,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Phantom Piper Mission Weight",
                DefaultValue = 20,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Higher value = more likely. The likelihood that the Phantom Piper will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightEnforcerGhost,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "Ethereal Enforcer Mission Weight",
                DefaultValue = 50,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Higher value = more likely. The likelihood that the Ethereal Enforcer will be the target.")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.WeightFiringSquad,
                Section = ConfigSections.Weight.GetDescription(),
                Key = "The Firing Squad Mission Weight",
                DefaultValue = 30,
                Description = new ConfigDescription("(Will be target only if Haunted Harpist mod is present.) Higher value = more likely. The likelihood that the The Firing Squad will be the target.")
            },

            ///////////////////////////////////////////////////////

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardLow,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Easy Mission Reward",
                DefaultValue = 50,
                Description = new ConfigDescription("How much the scrap dropped from an easy mission is worth (Snare Flea, Hoarding Bug).")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardMedium,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Medium Mission Reward",
                DefaultValue = 100,
                Description = new ConfigDescription("How much the scrap dropped from a medium mission is worth (Thumper, Bunker Spider, Masked, Butler, Blunderbug, Zombie, Ethereal Enforcer, Haunted Harpist).")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardHigh,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Hard Mission Reward",
                DefaultValue = 200,
                Description = new ConfigDescription("How much the scrap dropped from a hard mission is worth (Bracken, Nutcracker, Bug Mafia, Infestation, Baboon Gang, Stabbin' Bros, Facility Keeper, Phantom Piper).")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardExtreme,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Extreme Mission Reward",
                DefaultValue = 300,
                Description = new ConfigDescription("How much the scrap dropped from an extreme mission is worth (Good Boy, Last Month's Interns, Maneater, Last Year's Interns).")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardBrutal,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Brutal Mission Reward",
                DefaultValue = 400,
                Description = new ConfigDescription("How much the scrap dropped from an extreme mission is worth (Giant Size: Upgraded, Big Trouble Little Enemies, Who let the puppies out?, Zombie Apocalypse).")
            },
        };

        public static ConfigEntryBase[] ConfigEntries = new ConfigEntryBase[AllConfigurableSettings.Length];

        public static void BindConfigSettings()
        {
            for (int i = 0; i < AllConfigurableSettings.Length; i++)
            {
                Huntdown._logger.LogInfo($"[{i}] Binding: {AllConfigurableSettings[i].Key}");
                BindAnyObject(AllConfigurableSettings[i], i);
            }
        }

        private static void BindAnyObject(ConfigurableSetting setting, int index)
        {
            if (setting.DefaultValue is int || setting.DefaultValue is Enum)
            {
                ConfigEntries[index] = Bind(setting.Section, setting.Key, (int)setting.DefaultValue, setting.Description?.Description);
            }
            else if (setting.DefaultValue is bool)
            {
                ConfigEntries[index] = Bind(setting.Section, setting.Key, (bool)setting.DefaultValue, setting.Description?.Description);
            }
            else if (setting.DefaultValue is float)
            {
                ConfigEntries[index] = Bind(setting.Section, setting.Key, (float)setting.DefaultValue, setting.Description?.Description);
            }
            else
            {
                LogUnsupportedType(setting.Key);
            }
        }

        private static void LogUnsupportedType(string settingKey)
        {
            Huntdown._logger.LogError($"Unsupported DefaultValue type for setting {settingKey}.");
        }

        private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description = null)
        {
            return Huntdown._instance.Config.Bind(section, key, defaultValue, description);
        }
    }
}