using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArtifactsRandomizer
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.KingEnderBrine.ArtifactsRandomizer", "Artifacts Randomizer", "1.4.1")]
    public class ArtifactsRandomizerPlugin : BaseUnityPlugin
    {
        public enum Randomization
        {
            Weight,
            Chance
        }

        private static ConfigEntry<bool> IsEnabled { get; set; }
        private static ConfigEntry<string> Blacklist { get; set; }
        private static ConfigEntry<string> ArtifactWeights { get; set; }
        private static ConfigEntry<string> ArtifactChances { get; set; }
        private static ConfigEntry<int> MaxCount { get; set; }
        private static ConfigEntry<int> MinCount { get; set; }
        private static ConfigEntry<Randomization> RandomizationMode { get; set; }

        private static WeightedSelection<ArtifactIndex> ArtifactWeightsSelection {
            get
            {
                var selection = new WeightedSelection<ArtifactIndex>();
                var customWeights = new Dictionary<ArtifactIndex, int>();
                for (var i = 0; i < ArtifactCatalog.artifactCount; i++)
                {
                    customWeights[(ArtifactIndex)i] = 50;
                }
                ArtifactWeights.Value
                    .Split(new string[] { ", ", "," }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(el => el.Split(new char[] { ':', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
                    .Select(el =>
                    {
                        var index = ArtifactCatalog.FindArtifactIndex(el[0]);
                        customWeights[index] = int.Parse(el[1]);
                        return index;
                    }).ToArray();
                foreach(var row in customWeights)
                {
                    selection.AddChoice(row.Key, row.Value);
                }
                return selection;
            }
        }

        private static Dictionary<ArtifactIndex, float> ArtifactChanceDict
        {
            get
            {
                var chances = new Dictionary<ArtifactIndex, float>();
                for (var i = 0; i < ArtifactCatalog.artifactCount; i++)
                {
                    chances[(ArtifactIndex)i] = 0;
                }
                ArtifactChances.Value
                    .Split(new string[] { ", ", "," }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(el => el.Split(new char[] { ':', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
                    .Select(el =>
                    {
                        var index = ArtifactCatalog.FindArtifactIndex(el[0]);
                        Debug.Log(el[1]);
                        chances[index] = float.Parse(el[1], CultureInfo.InvariantCulture);
                        return index;
                    }).ToArray();
                return chances;
            }
        }

        private static ArtifactIndex[] BlacklistIndex => Blacklist.Value
            .Split(new char[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(el => ArtifactCatalog.FindArtifactIndex(el))
            .Where(el => el != ArtifactIndex.None)
            .ToArray();

        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();

            IsEnabled = Config.Bind("Main", "enabled", true, "Is mod should randomize artifacts or not");
            RandomizationMode = Config.Bind("Main", "randomizationMode", Randomization.Weight, "Randomization mode which will be used");
            Blacklist = Config.Bind("Main", "blacklist", "Artifact names (comma-separated) that should be ingored when randomizing", "");

            ArtifactChances = Config.Bind("Chance", "artifactChances", "", "Artifact chance with names (comma-separated).\nExample: `Bomb: 0.1, Command: 0.2`");
           
            ArtifactWeights = Config.Bind("Weght", "artifactWeights", "", "Artifact weight with names (comma-separated).\nExample: `Bomb: 20, Command: 50`");
            MaxCount = Config.Bind("Weght", "maxCount", -1, "Maximum count of artifacts to be enabled via randomization");
            MinCount = Config.Bind("Weght", "minCount", 0, "Minimum count of artifacts to be enabled via randomization");


            On.RoR2.Run.Start += (orig, self) =>
            {
                RandomizeRunArtifacts();
                orig(self);
            };

            On.RoR2.Run.AdvanceStage += (orig, self, nextScene) =>
            {
                RandomizeRunArtifacts();
                orig(self, nextScene);
            };
        }

        private static void RandomizeRunArtifacts()
        {
            if (!IsEnabled.Value)
            {
                return;
            }
            switch (RandomizationMode.Value)
            {
                case Randomization.Weight:
                    RandomizeByWeight();
                    break;
                case Randomization.Chance:
                    RandomizeByChance();
                    break;
            }
        }

        private static void RandomizeByWeight()
        {
            var selection = ArtifactWeightsSelection;
            foreach (var index in BlacklistIndex.OrderByDescending(el => el))
            {
                selection.RemoveChoice((int)index);
            }
            var maximum = Mathf.Max(0, Mathf.Min(selection.Count, MaxCount.Value == -1 ? selection.Count : MaxCount.Value));
            var minimum = Mathf.Max(0, Mathf.Min(MinCount.Value, maximum));
            var enabledCount = Random.Range(minimum, maximum + 1);
            for (var i = 1; i <= enabledCount; i++)
            {
                var index = selection.EvaluteToChoiceIndex(Random.Range(0F, 1F));
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef(selection.GetChoice(index).value), true);
                selection.RemoveChoice(index);
            }
            for (var i = 0; i < selection.Count; i++)
            {
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef(selection.GetChoice(i).value), false);
            }
        }

        private static void RandomizeByChance()
        {
            var blacklist = BlacklistIndex;
            var chances = ArtifactChanceDict.Where(el => !blacklist.Contains(el.Key));
            foreach (var chance in chances)
            {
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef(chance.Key), Random.Range(0F, 1F) < chance.Value);
            }
        }

        [ConCommand(commandName = "ar_enable", flags = ConVarFlags.None, helpText = "Enable artifacts randomization")]
        private static void CCEnable(ConCommandArgs args)
        {
            if (IsEnabled.Value)
            {
                return;
            }
            IsEnabled.Value = true;
            IsEnabled.ConfigFile.Save();
            Debug.Log($"[ArtifactsRandomizer] is enabled");
        }

        [ConCommand(commandName = "ar_disable", flags = ConVarFlags.None, helpText = "Disable artifacts randomization")]
        private static void CCDisable(ConCommandArgs args)
        {
            if (!IsEnabled.Value)
            {
                return;
            }
            IsEnabled.Value = false;
            IsEnabled.ConfigFile.Save();
            Debug.Log($"[ArtifactsRandomizer] is disabled");
        }

        [ConCommand(commandName = "ar_status", flags = ConVarFlags.None, helpText = "Shows is artifacts randomization enabled or disabled")]
        private static void CCStatus(ConCommandArgs args)
        {
            Debug.Log($"[ArtifactsRandomizer] is {(IsEnabled.Value ? "enabled" : "disabled")}");
        }
    }
}