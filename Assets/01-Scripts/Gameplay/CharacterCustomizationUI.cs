using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CharacterCustomization
{
    public class CustomizableCharacterUI : MonoBehaviour
    {
        [System.Serializable]

        public class SlotUI
        {
            public SlotType slotType; 
            public ScrollRect scrollView; 
            public Transform content; 
        }
        [Header("Equipped Items UI")]
        public ScrollRect equippedItemsScrollView; // Référence au ScrollView des objets équipés
        public Transform equippedItemsContent;

        [Header("Action Buttons")]
        public Button buttonSupprimer; // Référence au bouton "Supprimer"
        public Button buttonChangeColor;
        private SlotType _selectedSlotType; // Stocker le slot sélectionné

        [Header("Color Picker")]
        public ColorPicker colorPicker; // Référence au ColorPicker                   

 
        public List<SlotUI> slotUIs; 
        public GameObject buttonPrefab; 
        private CharacterCustomization _characterGameObject;
        public GameObject characterPrefab;
        public SlotLibrary slotLibrary; // Ajoutez ce champ pour la bibliothèque de slots

        private Dictionary<SlotType, GameObject> _equippedObjects = new Dictionary<SlotType, GameObject>();
        public void Initialize(CharacterCustomization character)
        {
            _characterGameObject = new CharacterCustomization(characterPrefab, slotLibrary);

            // Accéder au GameObject du personnage
            GameObject characterInstance = _characterGameObject.CharacterInstance;
            Debug.Log($"Personnage instancié : {characterInstance.name}");

            PopulateUI();
            UpdateEquippedItemsUI(); // Mettre à jour l'UI des objets équipés
        }


        private void PopulateUI()
        {
            foreach (var slotUI in slotUIs)
            {
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotUI.slotType);
                if (slot == null)
                {
                    Debug.Log($"Aucun slot trouvé pour {slotUI.slotType}");
                    continue;
                }

                foreach (Transform child in slotUI.content)
                {
                    Destroy(child.gameObject);
                }

                var prefabs = slot.GetAvailablePrefabs();
                Debug.Log($"Il y a {prefabs.Count()} prefabs disponibles pour le slot {slotUI.slotType}");

                foreach (var prefab in prefabs)
                {
                    GameObject buttonObj = Instantiate(buttonPrefab, slotUI.content);
                    buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = prefab.name;
                    Button button = buttonObj.GetComponent<Button>();

                    button.onClick.AddListener(() => EquipPrefab(slotUI.slotType, prefab));
                }
            }
        }


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
                        // Désactiver l'objet précédent
                        previousObject.SetActive(false);
                        Debug.Log($"Ancien objet pour {slotType} désactivé.");
                    }
                }

                // Appliquer le nouveau prefab
                slot.SetPrefab(prefab);
                slot.Toggle(true); // S'assurer que le slot est activé et visible

                // Utiliser slot.Preview pour obtenir l'objet cible
                GameObject targetObject = slot.Preview;
                if (targetObject != null)
                {
                    // Instancier une copie du Prefab pour éviter de modifier l'original
                    GameObject instance = Instantiate(targetObject);

                    // Définir le parent de l'instance
                    instance.transform.SetParent(_characterGameObject.CharacterInstance.transform);
                    instance.transform.localPosition = Vector3.zero; // Réinitialiser la position relative
                    instance.transform.localRotation = Quaternion.identity; // Réinitialiser la rotation
                    instance.transform.localScale = Vector3.one; // Réinitialiser l'échelle
                    instance.SetActive(true); // Activer le nouvel objet

                    // Stocker la référence du nouvel objet équipé
                    _equippedObjects[slotType] = instance;
                    Debug.Log($"Objet équipé pour {slotType} : {instance.name}"); // <-- Ajoutez ce log
                }

                Debug.Log($"Prefab changé pour {slotType} : {prefab.name}");
                _characterGameObject.RefreshCustomization(); // Force la mise à jour visuelle

                // Mettre à jour l'UI des objets équipés
                UpdateEquippedItemsUI(); // <-- Assurez-vous que cette ligne est appelée
            }
            else
            {
                Debug.LogWarning($"Slot {slotType} non trouvé lors du changement de prefab !");
            }
        }


        private void UnequipPrefab(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType];
                if (equippedObject != null)
                {
                    Destroy(equippedObject); // Détruire l'objet équipé
                }

                _equippedObjects.Remove(slotType); // Retirer l'objet du dictionnaire

                // Désactiver le slot dans le personnage
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotType);
                if (slot != null)
                {
                    slot.Toggle(false);
                }

                // Mettre à jour l'UI des objets équipés
                UpdateEquippedItemsUI();

                // Désactiver les boutons d'action
                buttonSupprimer.gameObject.SetActive(false);
                buttonChangeColor.gameObject.SetActive(false);
            }
        }

        private void OnChangeColor(SlotType slotType)
        {
            Debug.Log($"Changer la couleur pour le slot : {slotType}");

            // Vérifier si un objet est équipé pour ce slot
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType];

                // Récupérer le Renderer ou SkinnedMeshRenderer dans les enfants
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetTexture("_BaseMap", null);
                    // Ouvrir le ColorPicker avec la couleur actuelle du matériau
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                }
                else
                {
                    Debug.LogWarning($"Aucun Renderer ou SkinnedMeshRenderer trouvé pour l'objet équipé dans le slot : {slotType}");
                }
            }
            else
            {
                Debug.LogWarning($"Aucun objet équipé trouvé pour le slot : {slotType}");
            }
        }

        // Méthode appelée lorsque la couleur est modifiée dans le ColorPicker
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


        // Méthode appelée lorsque l'utilisateur confirme la sélection de couleur
        private void OnColorSelected(Color color)
        {
            Debug.Log($"Couleur sélectionnée : {color}");

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

            // Désactiver les boutons d'action après utilisation
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }


        private void UpdateEquippedItemsUI()
        {
            // Supprimer tous les enfants existants dans le content
            foreach (Transform child in equippedItemsContent)
            {
                Destroy(child.gameObject);
            }

            Debug.Log($"Nombre d'objets équipés : {_equippedObjects.Count}");

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
                        Debug.Log($"Bouton créé pour : {equippedItem.Value.name}");
                    }
                    else
                    {
                        Debug.LogWarning("TextMeshProUGUI non trouvé dans le bouton !");
                    }

                    Button button = buttonObj.GetComponent<Button>();
                    if (button != null)
                    {
                        // Stocker le slotType dans une variable locale pour le capturer dans le listener
                        SlotType slotType = equippedItem.Key;

                        // Ajouter un listener pour activer les boutons d'action
                        button.onClick.AddListener(() => OnEquippedItemClicked(slotType));
                    }
                    else
                    {
                        Debug.LogWarning("Bouton non trouvé dans le prefab !");
                    }
                }
                else
                {
                    Debug.LogWarning("Échec de l'instanciation du bouton !");
                }
            }

            // Désactiver les boutons d'action par défaut
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }
        private void OnEquippedItemClicked(SlotType slotType)
        {
            // Stocker le slotType sélectionné
            _selectedSlotType = slotType;

            // Activer les boutons d'action
            buttonSupprimer.gameObject.SetActive(true);
            buttonChangeColor.gameObject.SetActive(true); // <-- Renommez buttonAutreAction en buttonChangeColor

            // Configurer les actions des boutons
            buttonSupprimer.onClick.RemoveAllListeners();
            buttonSupprimer.onClick.AddListener(() => UnequipPrefab(_selectedSlotType));

            buttonChangeColor.onClick.RemoveAllListeners(); // <-- Renommez buttonAutreAction en buttonChangeColor
            buttonChangeColor.onClick.AddListener(() => OnChangeColor(_selectedSlotType)); // <-- Appeler OnChangeColor

            Debug.Log($"Boutons d'action activés pour le slot : {slotType}");
        }
    }
}
