using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CharacterCustomization
{
    public class CustomizableCharacterUI : MonoBehaviour
    {

        /// <summary>
        /// Gestion de l'interface utilisateur pour la personnalisation du personnage.
        /// </summary>

        [System.Serializable]
        public class SlotUI
        {
            public SlotType slotType;
            public ScrollRect scrollView;
            public Transform content;
        }

        [Header("UI des objets équipés")]
        public ScrollRect equippedItemsScrollView;
        public Transform equippedItemsContent;

        [Header("Boutons d'action")]
        public Button buttonSupprimer; 
        public Button buttonChangeColor;

        [Header("Color Picker")]
        public ColorPicker colorPicker;                  

        [Header("Configuration des slots et des éléments UI")]
        public List<SlotUI> slotUIs;
        public GameObject buttonPrefab;
        public SlotLibrary slotLibrary;

        private CharacterCustomization _characterGameObject;
        public GameObject characterPrefab;

        private Dictionary<SlotType, GameObject> _equippedObjects = new Dictionary<SlotType, GameObject>();
        private SlotType _selectedSlotType;


        /// <summary>
        /// Initialise l'UI avec un personnage donné.
        /// </summary>
        public void Initialize(CharacterCustomization characterCustomization)
        {
            _characterGameObject = characterCustomization;
            PopulateUI();
            UpdateEquippedItemsUI();
        }

        /// <summary>
        /// Génère l'UI des objets disponibles pour chaque slot.
        /// </summary>
        private void PopulateUI()
        {
            foreach (var slotUI in slotUIs)
            {
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotUI.slotType);
                if (slot == null) continue;

                // Nettoie les anciens boutons avant d'ajouter les nouveaux
                foreach (Transform child in slotUI.content)
                {
                    Destroy(child.gameObject);
                }


                // Création des boutons pour chaque objet disponible dans le slot
                foreach (var prefab in slot.GetAvailablePrefabs())
                {
                    GameObject buttonObj = Instantiate(buttonPrefab, slotUI.content);
                    buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = prefab.name;
                    Button button = buttonObj.GetComponent<Button>();
                    button.onClick.AddListener(() => EquipPrefab(slotUI.slotType, prefab));
                }
            }
        }

        /// <summary>
        /// Équipe un nouvel objet à un slot donné en supprimant l'ancien.
        /// </summary>
        private void EquipPrefab(SlotType slotType, GameObject prefab)
        {
            var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotType);
            if (slot != null)
            {
                // Vérifier si un objet est déjà équipé pour ce slot
                if (_equippedObjects.ContainsKey(slotType))
                {
                    GameObject previousObject = _equippedObjects[slotType];
                    if (previousObject != null)
                    {
                        // Détruire l'objet précédent
                        Destroy(previousObject);
                    }

                }

                // Appliquer le nouveau prefab
                slot.SetPrefab(prefab);
                slot.Toggle(true); 

                GameObject targetObject = slot.Preview;
                if (targetObject != null)
                {
                    // Instanciation du nouvel objet et positionnement correct
                    GameObject instance = Instantiate(targetObject);
                    instance.transform.SetParent(_characterGameObject.CharacterInstance.transform);
                    instance.transform.localPosition = Vector3.zero; 
                    instance.transform.localRotation = Quaternion.identity; 
                    instance.transform.localScale = Vector3.one; 
                    instance.SetActive(true);

                    // Stocker la référence du nouvel objet équipé
                    _equippedObjects[slotType] = instance;
                }

                _characterGameObject.RefreshCustomization(); 
                UpdateEquippedItemsUI(); 
            }
        }


        /// <summary>
        /// Retire un objet équipé d'un slot donné.
        /// </summary>
        private void UnequipPrefab(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType];
                if (equippedObject != null)
                {
                    Destroy(equippedObject); 
                }

                _equippedObjects.Remove(slotType); 

                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotType);
                if (slot != null)
                {
                    slot.Toggle(false);
                }

                UpdateEquippedItemsUI();
                buttonSupprimer.gameObject.SetActive(false);
                buttonChangeColor.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Ouvre le ColorPicker pour changer la couleur d'un objet équipé.
        /// </summary>
        private void OnChangeColor(SlotType slotType)
        {

            // Vérifier si un objet est équipé pour ce slot
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType];

                // Récupérer le Renderer dans les enfants
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetTexture("_BaseMap", null);
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                }
            }
        }

        /// <summary>
        /// Modifie temporairement la couleur de l'objet en temps réel.
        /// </summary>
        private void OnColorChanged(Color color)
        {
            if (_equippedObjects.ContainsKey(_selectedSlotType))
            {
                GameObject equippedObject = _equippedObjects[_selectedSlotType];
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // Modifier la couleur directement
                    renderer.material.color = color;
                }
            }
        }

        /// <summary>
        /// Applique la nouvelle couleur de manière définitive.
        /// </summary>
        private void OnColorSelected(Color color)
        {
            if (_equippedObjects.ContainsKey(_selectedSlotType))
            {
                GameObject equippedObject = _equippedObjects[_selectedSlotType];
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // Dupliquer le matériau pour éviter de modifier le matériau partagé
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color; // Appliquer la nouvelle couleur
                }
            }
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }

        /// <summary>
        /// Met à jour l'affichage des objets actuellement équipés.
        /// </summary>
        private void UpdateEquippedItemsUI()
        {
            // Supprimer tous les enfants existants dans le content
            foreach (Transform child in equippedItemsContent)
            {
                Destroy(child.gameObject);
            }

            // Parcourir tous les objets équipés et les afficher dans le ScrollView
            foreach (var equippedItem in _equippedObjects)
            {
                GameObject buttonObj = Instantiate(buttonPrefab, equippedItemsContent);
                if (buttonObj != null)
                {
                    var textMeshPro = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (textMeshPro != null)
                    {
                        textMeshPro.text = equippedItem.Value.name;
                    }

                    Button button = buttonObj.GetComponent<Button>();
                    if (button != null)
                    {
                        // Stocker le slotType dans une variable 
                        SlotType slotType = equippedItem.Key;
                        button.onClick.AddListener(() => OnEquippedItemClicked(slotType));
                    }
                }
            }
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }

        /// <summary>
        /// Gère l'interaction avec un objet équipé lorsqu'on clique dessus.
        /// </summary>
        private void OnEquippedItemClicked(SlotType slotType)
        {
            // Stocker le slotType sélectionné
            _selectedSlotType = slotType;

            buttonSupprimer.gameObject.SetActive(true);
            buttonChangeColor.gameObject.SetActive(true); 

            buttonSupprimer.onClick.RemoveAllListeners();
            buttonSupprimer.onClick.AddListener(() => UnequipPrefab(_selectedSlotType));

            buttonChangeColor.onClick.RemoveAllListeners(); 
            buttonChangeColor.onClick.AddListener(() => OnChangeColor(_selectedSlotType));
        }
    }
}
