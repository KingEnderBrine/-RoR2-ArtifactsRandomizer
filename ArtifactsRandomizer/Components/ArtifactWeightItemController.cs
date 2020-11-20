using RoR2;
using TMPro;
using UnityEngine;

namespace ArtifactsRandomizer.Components
{
    public class ArtifactWeightItemController : MonoBehaviour
    {
        public ArtifactWeightFieldController fieldController; 
        public TextMeshProUGUI text;
        public TMP_InputField inputField;
        [HideInInspector]
        public ArtifactIndex artifactIndex;

        public void OnValueChanged(string newValue)
        {
            if (!int.TryParse(newValue, out var number))
            {
                return;
            }
            if (number < 0)
            {
                inputField.text = "0";
                return;
            }
        }

        public void OnEndEdit(string newValue)
        {
            if (!int.TryParse(newValue, out var number))
            {
                inputField.SetTextWithoutNotify("0");
                number = 0;
            }
            fieldController?.ItemEndEdit(artifactIndex, number);
        }

        public void DeleteButtonClick()
        {
            fieldController?.DeleteItem(artifactIndex);
            Destroy(gameObject);
        }
    }
}
