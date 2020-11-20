using InLobbyConfig.Fields;
using System;
using System.Collections.Generic;

namespace ArtifactsRandomizer.Fields
{
    public abstract class BaseDictionaryField<TKey, TValue> : BaseConfigField<IDictionary<TKey, TValue>>
    {
        protected Action<TKey, TValue> OnItemAddedCallback { get; }
        protected Action<TKey> OnItemRemovedCallback { get; }
        protected Action<TKey, TValue> OnItemEndEditCallback { get; }

        public BaseDictionaryField(string displayName, Func<IDictionary<TKey, TValue>> valueAccessor, Action<TKey, TValue> onItemAdded, Action<TKey> onItemRemoved, Action<TKey, TValue> onItemEndEdit) : base(displayName, valueAccessor, null)
        {
            OnItemAddedCallback = onItemAdded;
            OnItemRemovedCallback = onItemRemoved;
            OnItemEndEditCallback = onItemEndEdit;
        }

        public BaseDictionaryField(string displayName, string tooltip, Func<IDictionary<TKey, TValue>> valueAccessor, Action<TKey, TValue> onItemAdded, Action<TKey> onItemRemoved, Action<TKey, TValue> onItemEndEdit) : base(displayName, tooltip, valueAccessor, null)
        {
            OnItemAddedCallback = onItemAdded;
            OnItemRemovedCallback = onItemRemoved;
            OnItemEndEditCallback = onItemEndEdit;
        }

        public void OnItemAdded(TKey key, TValue value)
        {
            OnItemAddedCallback?.Invoke(key, value);
        }

        public void OnItemRemoved(TKey key)
        {
            OnItemRemovedCallback?.Invoke(key);
        }

        public void OnItemEndEdit(TKey key, TValue value)
        {
            OnItemEndEditCallback?.Invoke(key, value);
        }
    }
}
