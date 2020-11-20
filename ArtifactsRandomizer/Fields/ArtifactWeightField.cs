using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtifactsRandomizer.Fields
{
    public class ArtifactWeightField : BaseDictionaryField<ArtifactIndex, int>
    {
        private static GameObject fieldPrefab;
        public override GameObject FieldPrefab => fieldPrefab ? fieldPrefab : (fieldPrefab = AssetBundleHelper.LoadPrefab("ArtifactWeightPrefab"));

        public ArtifactWeightField(string displayName, Func<IDictionary<ArtifactIndex, int>> valueAccessor, Action<ArtifactIndex, int> onItemAdded, Action<ArtifactIndex> onItemRemoved, Action<ArtifactIndex, int> onItemEndEdit) : base(displayName, valueAccessor, onItemAdded, onItemRemoved, onItemEndEdit)
        {
        }

        public ArtifactWeightField(string displayName, string tooltip, Func<IDictionary<ArtifactIndex, int>> valueAccessor, Action<ArtifactIndex, int> onItemAdded, Action<ArtifactIndex> onItemRemoved, Action<ArtifactIndex, int> onItemEndEdit) : base(displayName, tooltip, valueAccessor, onItemAdded, onItemRemoved, onItemEndEdit)
        {
        }
    }
}
