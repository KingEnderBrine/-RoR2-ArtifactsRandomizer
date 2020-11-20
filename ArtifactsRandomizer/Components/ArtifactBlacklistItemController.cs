using RoR2;
using TMPro;
using UnityEngine;

namespace ArtifactsRandomizer.Components
{
    public class ArtifactBlacklistItemController : MonoBehaviour
    {
        public TextMeshProUGUI text;
        [HideInInspector]
        public ArtifactIndex artifactIndex;

        public ArtifactBlacklistFieldController fieldController;

        public void DeleteButtonClick()
        {
            fieldController?.DeleteItem(transform.GetSiblingIndex());
            Destroy(gameObject);
        }
    }
}
