using ArtifactsRandomizer.Fields;
using BepInEx.Configuration;
using InLobbyConfig;
using InLobbyConfig.Fields;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static ArtifactsRandomizer.ArtifactsRandomizerPlugin;

namespace ArtifactsRandomizer
{
    internal static class ConfigHelper
    {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ConfigEntry<string> Blacklist { get; set; }
        public static ConfigEntry<string> ArtifactWeights { get; set; }
        public static ConfigEntry<int> DefaultWeight { get; set; }
        public static ConfigEntry<string> ArtifactChances { get; set; }
        public static ConfigEntry<float> DefaultChance { get; set; }
        public static ConfigEntry<int> MaxCount { get; set; }
        public static ConfigEntry<int> MinCount { get; set; }
        public static ConfigEntry<Randomization> RandomizationMode { get; set; }
        
        public static Dictionary<ArtifactIndex, float> ArtifactChancesDict { get; } = new Dictionary<ArtifactIndex, float>();
        public static Dictionary<ArtifactIndex, int> ArtifactWeightsDict { get; } = new Dictionary<ArtifactIndex, int>();
        public static List<ArtifactIndex> BlacklistIndices { get; } = new List<ArtifactIndex>();

        private static ModConfigEntry inLobbyConfigEntry;

        internal static void InitConfig(ConfigFile Config)
        {
            Enabled = Config.Bind("Main", nameof(Enabled), true, "Is mod should randomize artifacts or not");
            RandomizationMode = Config.Bind("Main", nameof(RandomizationMode), Randomization.Weight, "Randomization mode which will be used");
            Blacklist = Config.Bind("Main", nameof(Blacklist), "", "Artifact indices (comma-separated) that should be ingored when randomizing");

            DefaultChance = Config.Bind("Chance", nameof(DefaultChance), 0F, "Chance that will be applied to artifacts without custom chance");
            ArtifactChances = Config.Bind("Chance", nameof(ArtifactChances), "", "Artifact chance with indices (comma-separated).\nExample: `1: 0.1, 3: 0.2`");

            DefaultWeight = Config.Bind("Weight", nameof(DefaultWeight), 50, "Weight that will be applied to artifacts without custom weight");
            MaxCount = Config.Bind("Weight", nameof(MaxCount), -1, "Maximum count of artifacts to be enabled via randomization");
            MinCount = Config.Bind("Weight", nameof(MinCount), 0, "Minimum count of artifacts to be enabled via randomization");
            ArtifactWeights = Config.Bind("Weight", nameof(ArtifactWeights), "", "Artifact weight with indices (comma-separated).\nExample: `1: 20, 3: 50`");

            try
            {
                BlacklistIndices.Clear();
                BlacklistIndices.AddRange(Blacklist.Value
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(el => (ArtifactIndex)int.Parse(el.Trim()))
                    .Distinct());
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning("Failed to parse `Blacklist` config");
                InstanceLogger.LogError(e);
            }

            try
            {
                ArtifactWeightsDict.Clear();
                ArtifactWeights.Value
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(el => el.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(el =>
                        {
                            var index = (ArtifactIndex)int.Parse(el[0].Trim());
                            ArtifactWeightsDict[index] = int.Parse(el[1].Trim());
                            return index;
                        })
                        .ToArray();
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning("Failed to parse `ArtifactWeights` config");
                InstanceLogger.LogError(e);
            }

            try
            {
                ArtifactChancesDict.Clear();
                ArtifactChances.Value
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(el => el.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(el =>
                    {
                        var index = (ArtifactIndex)int.Parse(el[0].Trim());
                        ArtifactChancesDict[index] = float.Parse(el[1].Trim(), CultureInfo.InvariantCulture);
                        return index;
                    })
                    .ToArray();
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning("Failed to parse `ArtifactChances` config");
                InstanceLogger.LogError(e);
            }
        }

        internal static void InitInLobbyConfig()
        {
            inLobbyConfigEntry = new ModConfigEntry
            {
                DisplayName = "Artifacts Randomizer",
                EnableField = ConfigFieldUtilities.CreateFromBepInExConfigEntry(Enabled) as BooleanConfigField
            };
            inLobbyConfigEntry.SectionFields["Main"] = new List<IConfigField>
            {
                new EnumConfigField<Randomization>(RandomizationMode.Definition.Key, RandomizationMode.Description.Description, () => RandomizationMode.Value, (newValue) => RandomizationMode.Value = newValue),
                new SelectListField<ArtifactIndex>(Blacklist.Definition.Key, Blacklist.Description.Description, GetBlacklistIndices, AddBlacklistIndex, RemoveBlacklistIndex, GetArtifactOptions)
            };
            inLobbyConfigEntry.SectionFields["Chance"] = new List<IConfigField>
            {
                new FloatConfigField(DefaultChance.Definition.Key, DefaultChance.Description.Description, () => DefaultChance.Value, null, (newValue) => DefaultChance.Value = newValue, 0, 1),
                new ArtifactChanceField(ArtifactWeights.Definition.Key, ArtifactWeights.Description.Description, GetArtifactChances, AddArtifactChance, RemoveArtifactChance, EndEditArtifactChance)
            };
            inLobbyConfigEntry.SectionFields["Weight"] = new List<IConfigField>
            {
                new IntConfigField(DefaultWeight.Definition.Key, DefaultWeight.Description.Description, () => DefaultWeight.Value, null, (newValue) => DefaultWeight.Value = newValue, 0),
                new IntConfigField(MaxCount.Definition.Key, MaxCount.Description.Description, () => MaxCount.Value, null, (newValue) => MaxCount.Value = newValue, -1),
                new IntConfigField(MinCount.Definition.Key, MinCount.Description.Description, () => MinCount.Value, null, (newValue) => MinCount.Value = newValue, 0),
                new ArtifactWeightField(ArtifactWeights.Definition.Key, ArtifactWeights.Description.Description, GetArtifactWeights, AddArtifactWeight, RemoveArtifactWeight, EndEditArtifactWeight)
            };
            ModConfigCatalog.Add(inLobbyConfigEntry);
        }

        private static Dictionary<ArtifactIndex, string> GetArtifactOptions()
        {
            return ArtifactCatalog.artifactDefs.ToDictionary(el => el.artifactIndex, el => Language.GetString(el.nameToken));
        }

        private static List<ArtifactIndex> GetBlacklistIndices()
        {
            return BlacklistIndices;
        }

        private static void AddBlacklistIndex(ArtifactIndex item, int index)
        {
            BlacklistIndices.Insert(index, item);
            Blacklist.Value = string.Join(", ", BlacklistIndices);
        }

        private static void RemoveBlacklistIndex(int index)
        {
            BlacklistIndices.RemoveAt(index);
            Blacklist.Value = string.Join(", ", BlacklistIndices);
        }

        private static Dictionary<ArtifactIndex, int> GetArtifactWeights()
        {
            return ArtifactWeightsDict;
        }

        private static void AddArtifactWeight(ArtifactIndex key, int value)
        {
            ArtifactWeightsDict.Add(key, value);
            UpdateArtifactWeights();
        }

        private static void RemoveArtifactWeight(ArtifactIndex key)
        {
            ArtifactWeightsDict.Remove(key);
            UpdateArtifactWeights();
        }

        private static void EndEditArtifactWeight(ArtifactIndex key, int newValue)
        {
            ArtifactWeightsDict[key] = newValue;
            UpdateArtifactWeights();
        }

        private static void UpdateArtifactWeights()
        {
            var builder = new StringBuilder();
            foreach (var row in ArtifactWeightsDict)
            {
                builder.Append(row.Key).Append(": ").Append(row.Value).Append(", ");
            }
            ArtifactWeights.Value = builder.ToString();
        }

        private static Dictionary<ArtifactIndex, float> GetArtifactChances()
        {
            return ArtifactChancesDict;
        }

        private static void AddArtifactChance(ArtifactIndex key, float value)
        {
            ArtifactChancesDict.Add(key, value);
            UpdateArtifactChances();
        }

        private static void RemoveArtifactChance(ArtifactIndex key)
        {
            ArtifactChancesDict.Remove(key);
            UpdateArtifactChances();
        }

        private static void EndEditArtifactChance(ArtifactIndex key, float newValue)
        {
            ArtifactChancesDict[key] = newValue;
            UpdateArtifactChances();
        }

        private static void UpdateArtifactChances()
        {
            var builder = new StringBuilder();
            foreach (var row in ArtifactChancesDict)
            {
                builder.Append(row.Key).Append(": ").Append(row.Value.ToString(NumberFormatInfo.InvariantInfo)).Append(", ");
            }
            ArtifactChances.Value = builder.ToString();
        }
    }
}
