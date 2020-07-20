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
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.KingEnderBrine.ArtifactsRandomizer", "Artifacts Randomizer", "1.3.0")]
    public class ArtifactsRandomizerPlugin : BaseUnityPlugin
    {
        public enum Randomization
        {
            Weight,
            Chance
        }

        private static ConfigWrapper<bool> IsEnabled { get; set; }
        private static ConfigWrapper<string> Blacklist { get; set; }
        private static ConfigWrapper<string> ArtifactWeights { get; set; }
        private static ConfigWrapper<string> ArtifactChances { get; set; }
        private static ConfigWrapper<int> MaxCount { get; set; }
        private static ConfigWrapper<Randomization> RandomizationMode { get; set; }

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

            IsEnabled = Config.Wrap("Main", "enabled", "Is mod should randomize artifacts or not", true);
            RandomizationMode = Config.Wrap("Main", "randomizationMode", "Randomization mode which will be used", Randomization.Weight);

            ArtifactChances = Config.Wrap("Chance", "artifactChances", "Artifact chance with names (comma-separated).\nExample: `Bomb: 0.1, Command: 0.2`", "");
           
            Blacklist = Config.Wrap("Weght", "blacklist", "Artifact names (comma-separated) that should be ingored when randomizing", "");
            ArtifactWeights = Config.Wrap("Weght", "artifactWeights", "Artifact weight with names (comma-separated).\nExample: `Bomb: 20, Command: 50`", "");
            MaxCount = Config.Wrap("Weght", "maxCount", "Maximum count of artifacts to be enabled via randomization", -1);


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
            var enabledCount = Random.Range(0, Mathf.Max(1, Mathf.Min(selection.Count + 1, MaxCount.Value == -1 ? selection.Count + 1 : MaxCount.Value + 1)));
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