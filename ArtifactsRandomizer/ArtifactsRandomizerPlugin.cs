using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtifactsRandomizer
{
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.KingEnderBrine.ArtifactsRandomizer", "Artifacts Randomizer", "1.2.0")]
    public class ArtifactsRandomizerPlugin : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> isEnabled { get; set; }
        private static ConfigWrapper<string> blacklist { get; set; }
        private static ConfigWrapper<string> artifactWeights { get; set; }
        private static ConfigWrapper<int> maxCount { get; set; }

        private static WeightedSelection<ArtifactIndex> artifactWeightsSelection {
            get
            {
                var selection = new WeightedSelection<ArtifactIndex>();
                var customWeights = new Dictionary<ArtifactIndex, int>();
                for (var i = 0; i < ArtifactCatalog.artifactCount; i++)
                {
                    customWeights[(ArtifactIndex)i] = 50;
                }
                artifactWeights.Value
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

        private static ArtifactIndex[] blackListIndex => blacklist.Value
            .Split(new char[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(el => ArtifactCatalog.FindArtifactIndex(el))
            .Where(el => el != ArtifactIndex.None)
            .ToArray();

        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();

            isEnabled = Config.Wrap("Main", "enabled", "Is mod should randomize artifacts or not", true);

            blacklist = Config.Wrap("Main", "blacklist", "Artifact names (comma-separated) that should be ingored when randomizing", "");

            artifactWeights = Config.Wrap("Main", "artifactWeights", "Artifact weight with names (comma-separated).\nExample: `Bomb: 20, Command: 50`", "");
            
            maxCount = Config.Wrap("Main", "maxCount", "Maximum count of artifacts to be enabled via randomization", -1);

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
            if (!isEnabled.Value)
            {
                return;
            }
            var selection = artifactWeightsSelection;
            foreach (var index in blackListIndex.OrderByDescending(el => el))
            {
                selection.RemoveChoice((int)index);
            }
            var enabledCount = Random.Range(0, Mathf.Max(1, Mathf.Min(selection.Count + 1, maxCount.Value == -1 ? selection.Count + 1 : maxCount.Value + 1)));
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

        [ConCommand(commandName = "ar_enable", flags = ConVarFlags.None, helpText = "Enable artifacts randomization")]
        private static void CCEnable(ConCommandArgs args)
        {
            if (isEnabled.Value)
            {
                return;
            }
            isEnabled.Value = true;
            isEnabled.ConfigFile.Save();
            Debug.Log($"[ArtifactsRandomizer] is enabled");
        }

        [ConCommand(commandName = "ar_disable", flags = ConVarFlags.None, helpText = "Disable artifacts randomization")]
        private static void CCDisable(ConCommandArgs args)
        {
            if (!isEnabled.Value)
            {
                return;
            }
            isEnabled.Value = false;
            isEnabled.ConfigFile.Save();
            Debug.Log($"[ArtifactsRandomizer] is disabled");
        }

        [ConCommand(commandName = "ar_status", flags = ConVarFlags.None, helpText = "Shows is artifacts randomization enabled or disabled")]
        private static void CCStatus(ConCommandArgs args)
        {
            Debug.Log($"[ArtifactsRandomizer] is {(isEnabled.Value ? "enabled" : "disabled")}");
        }
    }
}