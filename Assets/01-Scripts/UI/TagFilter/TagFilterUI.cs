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
        public GameObject tagTogglePrefab;   // Prefab d�un toggle pour chaque tag
        public Transform tagContent;         // Conteneur pour les toggles dans le panneau

        [Header("References")]
        public CustomisationUIManager customisationUIManager; // R�f�rence � CustomisationUIManager

        private List<string> allTags = new List<string>(); // Liste de tous les tags possibles
        private List<string> selectedTags = new List<string>(); // Tags actuellement s�lectionn�s
        private bool _isUpdatingToggles = false; // Pour �viter les boucles

        private void Start()
        {
            if (tagPanel != null)
            {
                tagPanel.SetActive(false);
                Debug.Log("[TagFilterUI] tagPanel assign� et d�sactiv� au d�marrage.");
            }
            else
            {
                Debug.LogError("[TagFilterUI] tagPanel n'est PAS assign� dans l'inspecteur !");
            }

            if (filterButton != null)
            {
                filterButton.onClick.AddListener(OpenTagPanel);
                Debug.Log("[TagFilterUI] filterButton assign� et �v�nement li�.");
            }
            else
            {
                Debug.LogError("[TagFilterUI] filterButton n'est PAS assign� dans l'inspecteur !");
            }

            if (customisationUIManager != null)
            {
                Debug.Log("[TagFilterUI] customisationUIManager assign� : " + customisationUIManager.gameObject.name);
            }
            else
            {
                Debug.LogError("[TagFilterUI] customisationUIManager n'est PAS assign� dans l'inspecteur !");
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
            Debug.Log("[TagFilterUI] categorizedItems pr�t, appel de PopulateTagList.");
            PopulateTagList();
        }

        private void PopulateTagList()
        {
            if (tagContent == null || tagTogglePrefab == null)
            {
                Debug.LogError("[TagFilterUI] tagContent ou tagTogglePrefab n'est PAS assign� dans l'inspecteur !");
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
                    Debug.Log($"[TagFilterUI] Nombre de cat�gories trouv�es : {categorizedItems.Count}");
                    int itemCount = 0;
                    foreach (var category in categorizedItems)
                    {
                        Debug.Log($"[TagFilterUI] Cat�gorie : SlotType={category.Key.Item1}, GroupType={category.Key.Item2}, Nombre d'items : {category.Value.Count}");
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
                                            Debug.Log($"[TagFilterUI] Tag ajout� : {tag} pour l'item {item.itemName}");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"[TagFilterUI] Tag vide trouv� pour l'item {item.itemName}");
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"[TagFilterUI] Aucun tag d�fini pour l'item {item.itemName} ou tags est null");
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
            Debug.Log($"[TagFilterUI] Tags trouv�s : {string.Join(", ", allTags)}");

            if (allTags.Count == 0)
            {
                Debug.LogWarning("[TagFilterUI] Aucun tag trouv� ! V�rifiez les champs 'tags' des Items dans Assets/Resources/Items/.");
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
                    Debug.LogWarning($"[TagFilterUI] Aucun Toggle trouv� sur le prefab pour le tag {tag} !");
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
                Debug.Log($"[TagFilterUI] TagPanel {(isActive ? "activ�" : "d�sactiv�")}.");
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

            Debug.Log($"[TagFilterUI] Tag {tag} {(isOn ? "s�lectionn�" : "d�s�lectionn�")}. Tags s�lectionn�s : {string.Join(", ", selectedTags)}");

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

            Debug.Log("[TagFilterUI] Filtres r�initialis�s.");

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