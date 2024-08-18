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
        ToggleWeightDynamic
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

            ///////////////////////////////////////////////////////

            RewardLow,
            RewardMedium,
            RewardHigh,
            RewardExtreme
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
                Description = new ConfigDescription("How much the scrap dropped from a medium mission is worth (Thumper, Bunker Spider, Masked, Butler, Blunderbug).")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardHigh,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Hard Mission Reward",
                DefaultValue = 200,
                Description = new ConfigDescription("How much the scrap dropped from a hard mission is worth (Bracken, Nutcracker, Bug Mafia, Infestation).")
            },

            new ConfigurableSetting
            {
                Index = ConfigIndexes.RewardExtreme,
                Section = ConfigSections.Reward.GetDescription(),
                Key = "Extreme Mission Reward",
                DefaultValue = 300,
                Description = new ConfigDescription("How much the scrap dropped from an extreme mission is worth (Good Boy, Last Months Interns).")
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