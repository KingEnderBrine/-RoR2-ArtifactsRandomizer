using UnityEngine;
using RoR2;
using UnityEngine.Events;

namespace RoR2.UI
{
	public class UILayerKey : MonoBehaviour
	{
		public UILayer layer;
		public UnityEvent onBeginRepresentTopLayer;
		public UnityEvent onEndRepresentTopLayer;
	}
}
