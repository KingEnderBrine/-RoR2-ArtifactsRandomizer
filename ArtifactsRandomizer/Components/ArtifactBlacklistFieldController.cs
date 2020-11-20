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
    public class ArtifactBlacklistFieldController : ConfigFieldController
    {
        public TextMeshProUGUI fieldText;
        public TMP_Dropdown dropdown;
        public GameObject itemPrefab;
        public Button contentToggleButton;
        public Transform contentContainer;
        private ArtifactBlacklistField ConfigField { get; set; }
        private readonly List<ArtifactBlacklistItemController> items = new List<ArtifactBlacklistItemController>();

        public void Start()
        {
            ConfigField = configField as ArtifactBlacklistField;
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
                AddNewItem(item, true);
            }
            dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
        }

        public void AddNewItem(int index)
        {
            if (index >= dropdown.options.Count - 1)
            {
                return;
            }
            AddNewItem((ArtifactIndex)index);
            dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
            if (!contentContainer.gameObject.activeSelf)
            {
                contentToggleButton.onClick?.Invoke();
            }
        }

        public void AddNewItem(ArtifactIndex index, bool skipNotification = false)
        {
            if (ConfigField == null)
            {
                return;
            }
            if (items.Any(item => item.artifactIndex == index))
            {
                return;
            }
            var itemGameObject = Instantiate(itemPrefab, contentContainer);

            var itemController = itemGameObject.GetComponent<ArtifactBlacklistItemController>();
            itemController.text.text = (int)index >= dropdown.options.Count - 1 ? "{Missing artifact}" : dropdown.options[(int)index].text;
            itemController.artifactIndex = index;

            itemGameObject.SetActive(true);

            items.Add(itemController);
            if (skipNotification)
            {
                return;
            }
            ConfigField.OnItemAdded(index, itemGameObject.transform.GetSiblingIndex());
        }

        public void DeleteItem(int index)
        {
            if (ConfigField == null)
            {
                return;
            }
            items.RemoveAt(index);
            ConfigField.OnItemRemoved(index);
        }

        public void ToggleContent()
        {
            contentContainer.gameObject.SetActive(!contentContainer.gameObject.activeSelf);
        }
    }
}
