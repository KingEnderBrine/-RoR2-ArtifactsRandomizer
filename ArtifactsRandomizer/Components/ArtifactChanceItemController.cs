using RoR2;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace ArtifactsRandomizer.Components
{
    public class ArtifactChanceItemController : MonoBehaviour
    {
        public ArtifactChanceFieldController fieldController; 
        public TextMeshProUGUI text;
        public TMP_InputField inputField;
        [HideInInspector]
        public ArtifactIndex artifactIndex;

        public void OnValueChanged(string newValue)
        {
            if (!float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return;
            }
            if (number < 0)
            {
                inputField.text = "0";
                return;
            }
            if (number > 1)
            {
                inputField.text = "1";
                return;
            }
        }

        public void OnEndEdit(string newValue)
        {
            if (!float.TryParse(newValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var number))
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
