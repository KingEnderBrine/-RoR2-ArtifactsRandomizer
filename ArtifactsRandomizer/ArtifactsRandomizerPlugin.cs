using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ArtifactsRandomizer
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.KingEnderBrine.ProperSave", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.KingEnderBrine.InLobbyConfig")]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.KingEnderBrine.ArtifactsRandomizer", "Artifacts Randomizer", "2.0.0")]
    public class ArtifactsRandomizerPlugin : BaseUnityPlugin
    {
        public enum Randomization
        {
            Weight,
            Chance
        }

        private static bool IsProperSaveLoaded { get; set; }

        internal static ArtifactsRandomizerPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;

        public void Awake()
        {
            Instance = this;

            AssetBundleHelper.LoadAssetBundle();

            IsProperSaveLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");

            On.RoR2.Run.Start += RunStart;
            On.RoR2.Run.AdvanceStage += RunAdvanceStage;
        }

        public void Start()
        {
            ConfigHelper.InitConfig(Config);
            ConfigHelper.InitInLobbyConfig();
        }

        private static void RunStart(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            try
            {
                if (IsProperSaveLoaded && CheckProperSaveLoading())
                {
                    return;
                }
                RandomizeRunArtifacts();
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning("Failed to randomize artifacts");
                InstanceLogger.LogError(e);
            }
        }

        private static void RunAdvanceStage(On.RoR2.Run.orig_AdvanceStage orig, Run self, SceneDef nextScene)
        {
            try
            {
                RandomizeRunArtifacts();
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning("Failed to randomize artifacts");
                InstanceLogger.LogError(e);
            }
            orig(self, nextScene);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool CheckProperSaveLoading()
        {
            return ProperSave.Loading.IsLoading;
        }

        private static void RandomizeRunArtifacts()
        {
            if (!ConfigHelper.Enabled.Value)
            {
                return;
            }
            switch (ConfigHelper.RandomizationMode.Value)
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
            var selection = new WeightedSelection<ArtifactIndex>();
            foreach (var artifact in ArtifactCatalog.artifactDefs)
            {
                if (!string.IsNullOrWhiteSpace(artifact.unlockableName) && Run.instance.unlockablesUnlockedByAnyUser.Contains(artifact.unlockableName))
                {
                    continue;
                }
                if (ConfigHelper.BlacklistIndices.Contains(artifact.artifactIndex))
                {
                    continue;
                }
                if (!ConfigHelper.ArtifactWeightsDict.TryGetValue(artifact.artifactIndex, out var weight))
                {
                    weight = ConfigHelper.DefaultWeight.Value;
                }
                selection.AddChoice(artifact.artifactIndex, weight);
            }

            var maximum = Mathf.Max(0, Mathf.Min(selection.Count, ConfigHelper.MaxCount.Value == -1 ? selection.Count : ConfigHelper.MaxCount.Value));
            var minimum = Mathf.Max(0, Mathf.Min(ConfigHelper.MinCount.Value, maximum));
            var enabledCount = UnityEngine.Random.Range(minimum, maximum + 1);
            for (var i = 1; i <= enabledCount; i++)
            {
                var index = selection.EvaluteToChoiceIndex(UnityEngine.Random.Range(0F, 1F));
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
            foreach (var artifact in ArtifactCatalog.artifactDefs)
            {
                if (!string.IsNullOrWhiteSpace(artifact.unlockableName) && Run.instance.unlockablesUnlockedByAnyUser.Contains(artifact.unlockableName))
                {
                    continue;
                }
                if (ConfigHelper.BlacklistIndices.Contains(artifact.artifactIndex))
                {
                    continue;
                }
                if (ConfigHelper.ArtifactChancesDict.TryGetValue(artifact.artifactIndex, out var chance))
                {
                    RunArtifactManager.instance.SetArtifactEnabledServer(artifact, UnityEngine.Random.Range(0F, 1F) < chance);
                }
                else
                {
                    RunArtifactManager.instance.SetArtifactEnabledServer(artifact, UnityEngine.Random.Range(0F, 1F) < ConfigHelper.DefaultChance.Value);
                }
            }
        }
    }
}