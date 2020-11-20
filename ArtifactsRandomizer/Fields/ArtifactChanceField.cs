using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtifactsRandomizer.Fields
{
    public class ArtifactChanceField : BaseDictionaryField<ArtifactIndex, float>
    {
        private static GameObject fieldPrefab;
        public override GameObject FieldPrefab => fieldPrefab ? fieldPrefab : (fieldPrefab = AssetBundleHelper.LoadPrefab("ArtifactChancePrefab"));

        public ArtifactChanceField(string displayName, Func<IDictionary<ArtifactIndex, float>> valueAccessor, Action<ArtifactIndex, float> onItemAdded, Action<ArtifactIndex> onItemRemoved, Action<ArtifactIndex, float> onItemEndEdit) : base(displayName, valueAccessor, onItemAdded, onItemRemoved, onItemEndEdit)
        {
        }

        public ArtifactChanceField(string displayName, string tooltip, Func<IDictionary<ArtifactIndex, float>> valueAccessor, Action<ArtifactIndex, float> onItemAdded, Action<ArtifactIndex> onItemRemoved, Action<ArtifactIndex, float> onItemEndEdit) : base(displayName, tooltip, valueAccessor, onItemAdded, onItemRemoved, onItemEndEdit)
        {
        }
    }
}
