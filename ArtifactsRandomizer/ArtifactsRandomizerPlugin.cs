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
    [BepInPlugin("com.KingEnderBrine.ArtifactsRandomizer", "Artifacts Randomizer", "1.0.0")]
    public class ArtifactsRandomizerPlugin : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> isEnabled { get; set; }

        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();

            isEnabled = Config.Wrap("Main", "enabled", "Is mod should randomize artifacts or not", true);

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
            var enabledCount = Random.Range(0, ArtifactCatalog.artifactCount + 1);
            var range = Enumerable.Range(0, ArtifactCatalog.artifactCount).ToList();
            for (var i = 0; i <= enabledCount; i++)
            {
                var index = Random.Range(0, range.Count);
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef((ArtifactIndex)index), true);
                range.RemoveAt(index);
            }
            for (var i = 0; i < range.Count; i++)
            {
                RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.GetArtifactDef((ArtifactIndex)range[i]), false);
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