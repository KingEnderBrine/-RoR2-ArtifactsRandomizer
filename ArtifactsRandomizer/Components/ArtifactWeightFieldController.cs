using ArtifactsRandomizer.Fields;
using InLobbyConfig.FieldControllers;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsRandomizer.Components
{
    public class ArtifactWeightFieldController : ConfigFieldController
    {
        public TextMeshProUGUI fieldText;
        public TMP_Dropdown dropdown;
        public GameObject itemPrefab;
        public Button contentToggleButton;
        public Transform contentContainer;
        private ArtifactWeightField ConfigField { get; set; }
        private readonly Dictionary<ArtifactIndex, ArtifactWeightItemController> items = new Dictionary<ArtifactIndex, ArtifactWeightItemController>();

        public void Start()
        {
            ConfigField = configField as ArtifactWeightField;
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

        public void AddNewItem(ArtifactIndex key, int value, bool skipNotification = false)
        {
            if (ConfigField == null)
            {
                return;
            }
            if (items.ContainsKey(key))
            {
                return;
            }
            ArtifactsRandomizerPlugin.InstanceLogger.LogWarning(key);
            var itemGameObject = Instantiate(itemPrefab, contentContainer);

            var itemController = itemGameObject.GetComponent<ArtifactWeightItemController>();
            itemController.text.text = (int)key >= dropdown.options.Count - 1 ? "{Missing artifact}" : dropdown.options[(int)key].text;
            itemController.inputField.SetTextWithoutNotify(value.ToString());
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

        public void ItemEndEdit(ArtifactIndex key, int newValue)
        {
            ConfigField.OnItemEndEdit(key, newValue);
        }
    }
}
