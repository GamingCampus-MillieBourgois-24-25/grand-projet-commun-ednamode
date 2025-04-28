using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        public CustomisationUIManager customisationUIManager; // Référence à CustomisationUIManager

        private List<string> allTags = new List<string>(); // Liste de tous les tags possibles
        private List<string> selectedTags = new List<string>(); // Tags actuellement sélectionnés
        private bool _isUpdatingToggles = false; // Pour éviter les boucles

        private void Start()
        {

            StartCoroutine(InitializeTagList());
        }

        private IEnumerator InitializeTagList()
        {
            while (customisationUIManager == null || customisationUIManager.GetCategorizedItems() == null || customisationUIManager.GetCategorizedItems().Count == 0)
            {
                yield return null;
            }
            PopulateTagList();
        }

        private void PopulateTagList()
        {
            if (tagContent == null || tagTogglePrefab == null)
            {
                return;
            }

            foreach (Transform child in tagContent)
            {
                Destroy(child.gameObject);
            }

            HashSet<string> uniqueTags = new HashSet<string>();

            if (customisationUIManager != null)
            {
                var categorizedItems = customisationUIManager.GetCategorizedItems();
                if (categorizedItems != null)
                {
                    int itemCount = 0;
                    foreach (var category in categorizedItems)
                    {
                        foreach (var item in category.Value)
                        {
                            itemCount++;
                            if (item != null)
                            {
                                if (item.tags != null && item.tags.Count > 0)
                                {
                                    foreach (var tag in item.tags)
                                    {
                                        if (!string.IsNullOrEmpty(tag))
                                        {
                                            uniqueTags.Add(tag);
                                        }
                                        
                                    }
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
                bool isActive = !tagPanel.activeSelf;
                tagPanel.SetActive(isActive);
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


            if (customisationUIManager != null)
            {
                customisationUIManager.ApplyTagFilter(selectedTags);
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


            if (customisationUIManager != null)
            {
                customisationUIManager.ApplyTagFilter(selectedTags);
            }
            
        }
    }
}