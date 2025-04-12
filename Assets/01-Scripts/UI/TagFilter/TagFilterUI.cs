using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterCustomization
{
    public class TagFilterUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button filterButton;           // Bouton pour ouvrir le panneau de tags
        public GameObject tagPanel;          // Panneau contenant les tags
        public GameObject tagTogglePrefab;   // Prefab d’un toggle pour chaque tag
        public Transform tagContent;         // Conteneur pour les toggles dans le panneau

        [Header("References")]
        public CustomizableCharacterUI characterUI; // Référence à CustomizableCharacterUI

        private List<string> allTags = new List<string>(); // Liste de tous les tags possibles
        private List<string> selectedTags = new List<string>(); // Tags actuellement sélectionnés

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

            // Récupérer les tags directement depuis les Items dans clothingItems
            foreach (var item in characterUI.clothingItems)
            {
                if (item != null && item.tags != null)
                {
                    foreach (var tag in item.tags)
                    {
                        uniqueTags.Add(tag);
                    }
                }
            }

            allTags = new List<string>(uniqueTags);

            // Créer les toggles pour chaque tag
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
                    toggle.onValueChanged.AddListener((isOn) => OnTagToggleChanged(tag, isOn));
                }
            }
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
            selectedTags.Clear();
            if (tagContent != null)
            {
                foreach (Toggle toggle in tagContent.GetComponentsInChildren<Toggle>())
                {
                    toggle.isOn = false;
                }
            }
            if (characterUI != null)
            {
                characterUI.ApplyTagFilter(selectedTags);
            }
        }
    }
}