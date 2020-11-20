using ArtifactsRandomizer.Fields;
using InLobbyConfig.FieldControllers;
using RoR2;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsRandomizer.Components
{
    public class ArtifactChanceFieldController : ConfigFieldController
    {
        public TextMeshProUGUI fieldText;
        public TMP_Dropdown dropdown;
        public GameObject itemPrefab;
        public Button contentToggleButton;
        public Transform contentContainer;
        private ArtifactChanceField ConfigField { get; set; }
        private readonly Dictionary<ArtifactIndex, ArtifactChanceItemController> items = new Dictionary<ArtifactIndex, ArtifactChanceItemController>();

        public void Start()
        {
            ConfigField = configField as ArtifactChanceField;
            if (ConfigField == null)
            {
                return;
            }

            dropdown.options = ArtifactCatalog.artifactDefs.Select(def => new TMP_Dropdown.OptionData(Language.GetString(def.nameToken))).ToList();
            dropdown.options.Add(new TMP_Dropdown.OptionData());

            if (tooltipProvider)
            {
                tooltipProvider.SetContent(ConfigField.Tooltip);
            }

            if (fieldText)
            {
                fieldText.text = ConfigField.DisplayName;
            }

            var value = ConfigField.GetValue();
            foreach (var item in value)
            {
                AddNewItem(item.Key, item.Value, true);
            }
            dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
        }

        public void AddNewItem(int index)
        {
            if (index >= dropdown.options.Count - 1)
            {
                return;
            }
            AddNewItem((ArtifactIndex)index, 0);
            dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
            if (!contentContainer.gameObject.activeSelf)
            {
                contentToggleButton.onClick?.Invoke();
            }
        }

        public void AddNewItem(ArtifactIndex key, float value, bool skipNotification = false)
        {
            if (ConfigField == null)
            {
                return;
            }
            if (items.ContainsKey(key))
            {
                return;
            }
            var itemGameObject = Instantiate(itemPrefab, contentContainer);

            var itemController = itemGameObject.GetComponent<ArtifactChanceItemController>();
            itemController.text.text = (int)key >= dropdown.options.Count - 1 ? "{Missing artifact}" : dropdown.options[(int)key].text;
            itemController.inputField.SetTextWithoutNotify(value.ToString(NumberFormatInfo.InvariantInfo));
            itemController.artifactIndex = key;

            itemGameObject.SetActive(true);

            items.Add(key, itemController);
            if (skipNotification)
            {
                return;
            }
            ConfigField.OnItemAdded(key, value);
        }

        public void DeleteItem(ArtifactIndex key)
        {
            if (ConfigField == null)
            {
                return;
            }
            items.Remove(key);
            ConfigField.OnItemRemoved(key);
        }

        public void ToggleContent()
        {
            contentContainer.gameObject.SetActive(!contentContainer.gameObject.activeSelf);
        }

        public void ItemEndEdit(ArtifactIndex key, float newValue)
        {
            ConfigField.OnItemEndEdit(key, newValue);
        }
    }
}
