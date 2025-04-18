using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterCustomization
{
    public class TagFilterUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button filterButton;
        public GameObject tagPanel;       
        public GameObject tagTogglePrefab;   
        public Transform tagContent;         

        [Header("References")]
        public CustomizableCharacterUI characterUI; 

        private List<string> allTags = new List<string>(); 
        private List<string> selectedTags = new List<string>(); 
        private bool _isUpdatingToggles = false; 

        private void Start()
        {
            if (tagPanel != null) tagPanel.SetActive(false);

            if (filterButton != null)
            {
                filterButton.onClick.AddListener(OpenTagPanel);
            }

            PopulateTagList();
        }

        private void PopulateTagList()
        {
            HashSet<string> uniqueTags = new HashSet<string>();

            if (characterUI != null && characterUI.slotLibrary != null)
            {
                foreach (var slotEntry in characterUI.slotLibrary.Slots)
                {
                    foreach (var group in slotEntry.Groups)
                    {
                        foreach (var item in group.Items)
                        {
                            if (item != null && item.tags != null)
                            {
                                foreach (var tag in item.tags)
                                {
                                    uniqueTags.Add(tag);
                                }
                            }
                        }
                    }
                }
            }
          

            allTags = new List<string>(uniqueTags);

            foreach (var tag in allTags)
            {
                GameObject toggleObj = Instantiate(tagTogglePrefab, tagContent);
                Toggle toggle = toggleObj.GetComponent<Toggle>();
                if (toggle != null)
                {
                    TMPro.TextMeshProUGUI label = toggleObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = tag;
                    }
                    toggle.onValueChanged.AddListener((isOn) =>
                    {
                        if (_isUpdatingToggles) return; 
                        OnTagToggleChanged(tag, isOn);
                    });
                }
            }

            ClearFilters();
        }

        private void OpenTagPanel()
        {
            if (tagPanel != null)
            {
                tagPanel.SetActive(!tagPanel.activeSelf);
            }
        }

        private void OnTagToggleChanged(string tag, bool isOn)
        {
            if (isOn)
            {
                if (!selectedTags.Contains(tag))
                {
                    selectedTags.Add(tag);
                }
            }
            else
            {
                selectedTags.Remove(tag);
            }


            if (characterUI != null)
            {
                characterUI.ApplyTagFilter(selectedTags);
            }
        }

        public void ClearFilters()
        {
            _isUpdatingToggles = true; 
            selectedTags.Clear();
            if (tagContent != null)
            {
                foreach (Toggle toggle in tagContent.GetComponentsInChildren<Toggle>())
                {
                    toggle.isOn = false;
                }
            }
            _isUpdatingToggles = false;

            if (characterUI != null)
            {
                characterUI.ApplyTagFilter(selectedTags);
            }
        }
    }
}