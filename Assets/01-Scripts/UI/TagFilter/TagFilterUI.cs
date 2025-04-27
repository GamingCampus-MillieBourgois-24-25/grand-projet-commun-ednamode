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
            if (tagPanel != null)
            {
                tagPanel.SetActive(false);
                Debug.Log("[TagFilterUI] tagPanel assigné et désactivé au démarrage.");
            }
            else
            {
                Debug.LogError("[TagFilterUI] tagPanel n'est PAS assigné dans l'inspecteur !");
            }

            if (filterButton != null)
            {
                filterButton.onClick.AddListener(OpenTagPanel);
                Debug.Log("[TagFilterUI] filterButton assigné et événement lié.");
            }
            else
            {
                Debug.LogError("[TagFilterUI] filterButton n'est PAS assigné dans l'inspecteur !");
            }

            if (customisationUIManager != null)
            {
                Debug.Log("[TagFilterUI] customisationUIManager assigné : " + customisationUIManager.gameObject.name);
            }
            else
            {
                Debug.LogError("[TagFilterUI] customisationUIManager n'est PAS assigné dans l'inspecteur !");
            }

            StartCoroutine(InitializeTagList());
        }

        private IEnumerator InitializeTagList()
        {
            Debug.Log("[TagFilterUI] Attente de l'initialisation de CustomisationUIManager...");
            while (customisationUIManager == null || customisationUIManager.GetCategorizedItems() == null || customisationUIManager.GetCategorizedItems().Count == 0)
            {
                yield return null;
            }
            Debug.Log("[TagFilterUI] categorizedItems prêt, appel de PopulateTagList.");
            PopulateTagList();
        }

        private void PopulateTagList()
        {
            if (tagContent == null || tagTogglePrefab == null)
            {
                Debug.LogError("[TagFilterUI] tagContent ou tagTogglePrefab n'est PAS assigné dans l'inspecteur !");
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
                    Debug.Log($"[TagFilterUI] Nombre de catégories trouvées : {categorizedItems.Count}");
                    int itemCount = 0;
                    foreach (var category in categorizedItems)
                    {
                        Debug.Log($"[TagFilterUI] Catégorie : SlotType={category.Key.Item1}, GroupType={category.Key.Item2}, Nombre d'items : {category.Value.Count}");
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
                                            Debug.Log($"[TagFilterUI] Tag ajouté : {tag} pour l'item {item.itemName}");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"[TagFilterUI] Tag vide trouvé pour l'item {item.itemName}");
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"[TagFilterUI] Aucun tag défini pour l'item {item.itemName} ou tags est null");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("[TagFilterUI] Item null dans categorizedItems !");
                            }
                        }
                    }
                    Debug.Log($"[TagFilterUI] Total d'items parcourus : {itemCount}");
                }
                else
                {
                    Debug.LogError("[TagFilterUI] categorizedItems est null dans PopulateTagList !");
                }
            }
            else
            {
                Debug.LogError("[TagFilterUI] customisationUIManager est null dans PopulateTagList !");
            }

            allTags = new List<string>(uniqueTags);
            Debug.Log($"[TagFilterUI] Tags trouvés : {string.Join(", ", allTags)}");

            if (allTags.Count == 0)
            {
                Debug.LogWarning("[TagFilterUI] Aucun tag trouvé ! Vérifiez les champs 'tags' des Items dans Assets/Resources/Items/.");
            }

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
                else
                {
                    Debug.LogWarning($"[TagFilterUI] Aucun Toggle trouvé sur le prefab pour le tag {tag} !");
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
                Debug.Log($"[TagFilterUI] TagPanel {(isActive ? "activé" : "désactivé")}.");
            }
            else
            {
                Debug.LogError("[TagFilterUI] tagPanel est null dans OpenTagPanel !");
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

            Debug.Log($"[TagFilterUI] Tag {tag} {(isOn ? "sélectionné" : "désélectionné")}. Tags sélectionnés : {string.Join(", ", selectedTags)}");

            if (customisationUIManager != null)
            {
                customisationUIManager.ApplyTagFilter(selectedTags);
            }
            else
            {
                Debug.LogWarning("[TagFilterUI] customisationUIManager est null lors de OnTagToggleChanged !");
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
            else
            {
                Debug.LogWarning("[TagFilterUI] tagContent est null lors de ClearFilters !");
            }
            _isUpdatingToggles = false;

            Debug.Log("[TagFilterUI] Filtres réinitialisés.");

            if (customisationUIManager != null)
            {
                customisationUIManager.ApplyTagFilter(selectedTags);
            }
            else
            {
                Debug.LogWarning("[TagFilterUI] customisationUIManager est null lors de ClearFilters !");
            }
        }
    }
}