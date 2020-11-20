using InLobbyConfig.Fields;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtifactsRandomizer.Fields
{
    public class ArtifactBlacklistField : BaseConfigField<ICollection<ArtifactIndex>>
    {
        private static GameObject fieldPrefab;
        public override GameObject FieldPrefab => fieldPrefab ? fieldPrefab : (fieldPrefab = AssetBundleHelper.LoadPrefab("ArtifactBlacklistPrefab"));

        protected Action<ArtifactIndex, int> OnItemAddedCallback { get; }
        protected Action<int> OnItemRemovedCallback { get; }

        public ArtifactBlacklistField(string displayName, Func<ICollection<ArtifactIndex>> valueAccessor, Action<ArtifactIndex, int> onItemAdded, Action<int> onItemRemoved) : base(displayName, valueAccessor, null)
        {
            OnItemAddedCallback = onItemAdded;
            OnItemRemovedCallback = onItemRemoved;
        }

        public ArtifactBlacklistField(string displayName, string tooltip, Func<ICollection<ArtifactIndex>> valueAccessor, Action<ArtifactIndex, int> onItemAdded, Action<int> onItemRemoved) : base(displayName, tooltip, valueAccessor, null)
        {
            OnItemAddedCallback = onItemAdded;
            OnItemRemovedCallback = onItemRemoved;
        }

        public void OnItemAdded(ArtifactIndex value, int index)
        {
            OnItemAddedCallback?.Invoke(value, index);
        }

        public void OnItemRemoved(int index)
        {
            OnItemRemovedCallback?.Invoke(index);
        }
    }
}
