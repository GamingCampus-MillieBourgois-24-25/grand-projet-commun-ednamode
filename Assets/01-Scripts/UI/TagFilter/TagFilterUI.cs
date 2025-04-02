using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
            // Désactiver le panneau au démarrage
            tagPanel.SetActive(false);

            // Ajouter l’action au bouton de filtre
            filterButton.onClick.AddListener(OpenTagPanel);

            // Remplir la liste des tags disponibles
            PopulateTagList();
        }

        private void PopulateTagList()
        {
            if (characterUI == null)
            {
                Debug.LogError("characterUI est null dans TagFilterUI !");
                return;
            }
            if (characterUI.slotLibrary == null)
            {
                Debug.LogError("slotLibrary est null dans characterUI !");
                return;
            }
            if (tagTogglePrefab == null)
            {
                Debug.LogError("tagTogglePrefab est null dans TagFilterUI !");
                return;
            }
            if (tagContent == null)
            {
                Debug.LogError("tagContent est null dans TagFilterUI !");
                return;
            }

            HashSet<string> uniqueTags = new HashSet<string>();

            foreach (var slotEntry in characterUI.slotLibrary.Slots)
            {
                foreach (var prefab in slotEntry.Prefabs)
                {
                    ItemsSprite itemSprite = prefab.GetComponent<ItemsSprite>();
                    if (itemSprite != null && itemSprite.Tags != null)
                    {
                        foreach (var tag in itemSprite.Tags)
                        {
                            uniqueTags.Add(tag);
                        }
                    }
                }
            }

            foreach (var fullBodyEntry in characterUI.slotLibrary.FullBodyCostumes)
            {
                foreach (var slot in fullBodyEntry.Slots)
                {
                    ItemsSprite itemSprite = slot.GameObject.GetComponent<ItemsSprite>();
                    if (itemSprite != null && itemSprite.Tags != null)
                    {
                        foreach (var tag in itemSprite.Tags)
                        {
                            uniqueTags.Add(tag);
                        }
                    }
                }
            }

            allTags = uniqueTags.ToList();
            Debug.Log($"Nombre de tags trouvés : {allTags.Count}");

            foreach (var tag in allTags)
            {
                GameObject toggleObj = Instantiate(tagTogglePrefab, tagContent);
                if (toggleObj == null)
                {
                    Debug.LogError("Échec de l’instanciation de tagTogglePrefab !");
                    continue;
                }

                Toggle toggle = toggleObj.GetComponent<Toggle>();
                if (toggle == null)
                {
                    Debug.LogError($"toggleObj {toggleObj.name} n’a pas de composant Toggle !");
                    continue;
                }

                TMPro.TextMeshProUGUI label = toggleObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = tag;
                }
                else
                {
                    Debug.LogWarning($"Aucun TextMeshProUGUI trouvé dans {toggleObj.name}");
                }

                toggle.onValueChanged.AddListener((isOn) => OnTagToggleChanged(tag, isOn));
            }
        }

        private void OpenTagPanel()
        {
            tagPanel.SetActive(!tagPanel.activeSelf); // Ouvre ou ferme le panneau
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

            // Mettre à jour l’UI avec le filtre
            characterUI.ApplyTagFilter(selectedTags);
        }

        public void ClearFilters()
        {
            selectedTags.Clear();
            foreach (Toggle toggle in tagContent.GetComponentsInChildren<Toggle>())
            {
                toggle.isOn = false;
            }
            characterUI.ApplyTagFilter(selectedTags);
        }
    }
}