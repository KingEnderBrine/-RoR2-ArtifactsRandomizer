using UnityEngine.UI;
using UnityEngine.Events;

namespace RoR2.UI
{
	public class MPButton : Button
	{
		public bool allowAllEventSystems;
		public bool submitOnPointerUp;
		public bool disablePointerClick;
		public bool disableGamepadClick;
		public UILayerKey requiredTopLayer;
		public UnityEvent onFindSelectableLeft;
		public UnityEvent onFindSelectableRight;
		public UnityEvent onSelect;
		public UnityEvent onDeselect;
		public bool defaultFallbackButton;
	}
}
