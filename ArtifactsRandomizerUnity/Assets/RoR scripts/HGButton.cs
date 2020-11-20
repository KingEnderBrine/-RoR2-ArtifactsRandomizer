using UnityEngine.UI;

namespace RoR2.UI
{
	public class HGButton : MPButton
	{
		public bool showImageOnHover;
		public Image imageOnHover;
		public Image imageOnInteractable;
		public bool updateTextOnHover;
		//public LanguageTextMeshController hoverLanguageTextMeshController;
		public string hoverToken;
		public string uiClickSoundOverride;
	}
}
