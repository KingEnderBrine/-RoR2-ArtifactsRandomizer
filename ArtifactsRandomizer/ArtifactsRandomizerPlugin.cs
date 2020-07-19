using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Linq;
using UnityEngine;

namespace ArtifactsRandomizer
{
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.KingEnderBrine.ArtifactsRandomizer", "Artifacts Randomizer", "1.1.0")]
    public class ArtifactsRandomizerPlugin : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> isEnabled { get; set; }
        private static ConfigWrapper<string> blacklist { get; set; }

        private static ArtifactIndex[] blackListIndex => blacklist.Value
            .Split(new char[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(el => ArtifactCatalog.FindArtifactIndex(el))
            .Where(el => el != ArtifactIndex.None)
            .ToArray();

        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();

            isEnabled = Config.Wrap("Main", "enabled", "Is mod should randomize artifacts or not", true);

            blacklist = Config.Wrap("Main", "blacklist", "Artifact names (comma separated) that should be ingored when randomizing", "");

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
            var range = Enumerable.Range(0, ArtifactCatalog.artifactCount).Select(el => (ArtifactIndex)el).ToList();
            range = range.Except(blackListIndex).ToList();
            var enabledCount = Random.Range(0, range.Count + 1);
            for (var i = 1; i <= enabledCount; i++)
            {
                var index = Random.Range(0, range.Count);
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef(range[index]), true);
                range.RemoveAt(index);
            }
            for (var i = 0; i < range.Count; i++)
            {
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef(range[i]), false);
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