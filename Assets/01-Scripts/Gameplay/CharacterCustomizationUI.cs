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
        public ScrollRect equippedItemsScrollView; // R�f�rence au ScrollView des objets �quip�s
        public Transform equippedItemsContent;

        [Header("Action Buttons")]
        public Button buttonSupprimer; // R�f�rence au bouton "Supprimer"
        public Button buttonChangeColor;
        private SlotType _selectedSlotType; // Stocker le slot s�lectionn�

        [Header("Color Picker")]
        public ColorPicker colorPicker; // R�f�rence au ColorPicker                   

 
        public List<SlotUI> slotUIs; 
        public GameObject buttonPrefab; 
        private CharacterCustomization _characterGameObject;
        public GameObject characterPrefab;
        public SlotLibrary slotLibrary; // Ajoutez ce champ pour la biblioth�que de slots

        private Dictionary<SlotType, GameObject> _equippedObjects = new Dictionary<SlotType, GameObject>();
        public void Initialize(CharacterCustomization character)
        {
            _characterGameObject = new CharacterCustomization(characterPrefab, slotLibrary);

            // Acc�der au GameObject du personnage
            GameObject characterInstance = _characterGameObject.CharacterInstance;
            Debug.Log($"Personnage instanci� : {characterInstance.name}");

            PopulateUI();
            UpdateEquippedItemsUI(); // Mettre � jour l'UI des objets �quip�s
        }


        private void PopulateUI()
        {
            foreach (var slotUI in slotUIs)
            {
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotUI.slotType);
                if (slot == null)
                {
                    Debug.Log($"Aucun slot trouv� pour {slotUI.slotType}");
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
                // V�rifier si un objet est d�j� �quip� pour ce slot
                if (_equippedObjects.ContainsKey(slotType))
                {
                    GameObject previousObject = _equippedObjects[slotType];
                    if (previousObject != null)
                    {
                        // D�sactiver l'objet pr�c�dent
                        previousObject.SetActive(false);
                        Debug.Log($"Ancien objet pour {slotType} d�sactiv�.");
                    }
                }

                // Appliquer le nouveau prefab
                slot.SetPrefab(prefab);
                slot.Toggle(true); // S'assurer que le slot est activ� et visible

                // Utiliser slot.Preview pour obtenir l'objet cible
                GameObject targetObject = slot.Preview;
                if (targetObject != null)
                {
                    // Instancier une copie du Prefab pour �viter de modifier l'original
                    GameObject instance = Instantiate(targetObject);

                    // D�finir le parent de l'instance
                    instance.transform.SetParent(_characterGameObject.CharacterInstance.transform);
                    instance.transform.localPosition = Vector3.zero; // R�initialiser la position relative
                    instance.transform.localRotation = Quaternion.identity; // R�initialiser la rotation
                    instance.transform.localScale = Vector3.one; // R�initialiser l'�chelle
                    instance.SetActive(true); // Activer le nouvel objet

                    // Stocker la r�f�rence du nouvel objet �quip�
                    _equippedObjects[slotType] = instance;
                    Debug.Log($"Objet �quip� pour {slotType} : {instance.name}"); // <-- Ajoutez ce log
                }

                Debug.Log($"Prefab chang� pour {slotType} : {prefab.name}");
                _characterGameObject.RefreshCustomization(); // Force la mise � jour visuelle

                // Mettre � jour l'UI des objets �quip�s
                UpdateEquippedItemsUI(); // <-- Assurez-vous que cette ligne est appel�e
            }
            else
            {
                Debug.LogWarning($"Slot {slotType} non trouv� lors du changement de prefab !");
            }
        }


        private void UnequipPrefab(SlotType slotType)
        {
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType];
                if (equippedObject != null)
                {
                    Destroy(equippedObject); // D�truire l'objet �quip�
                }

                _equippedObjects.Remove(slotType); // Retirer l'objet du dictionnaire

                // D�sactiver le slot dans le personnage
                var slot = _characterGameObject.Slots.FirstOrDefault(s => s.Type == slotType);
                if (slot != null)
                {
                    slot.Toggle(false);
                }

                // Mettre � jour l'UI des objets �quip�s
                UpdateEquippedItemsUI();

                // D�sactiver les boutons d'action
                buttonSupprimer.gameObject.SetActive(false);
                buttonChangeColor.gameObject.SetActive(false);
            }
        }

        private void OnChangeColor(SlotType slotType)
        {
            Debug.Log($"Changer la couleur pour le slot : {slotType}");

            // V�rifier si un objet est �quip� pour ce slot
            if (_equippedObjects.ContainsKey(slotType))
            {
                GameObject equippedObject = _equippedObjects[slotType];

                // R�cup�rer le Renderer ou SkinnedMeshRenderer dans les enfants
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetTexture("_BaseMap", null);
                    // Ouvrir le ColorPicker avec la couleur actuelle du mat�riau
                    Color currentColor = renderer.material.color;
                    ColorPicker.Create(currentColor, "Choisissez une couleur", renderer, OnColorChanged, OnColorSelected);
                }
                else
                {
                    Debug.LogWarning($"Aucun Renderer ou SkinnedMeshRenderer trouv� pour l'objet �quip� dans le slot : {slotType}");
                }
            }
            else
            {
                Debug.LogWarning($"Aucun objet �quip� trouv� pour le slot : {slotType}");
            }
        }

        // M�thode appel�e lorsque la couleur est modifi�e dans le ColorPicker
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


        // M�thode appel�e lorsque l'utilisateur confirme la s�lection de couleur
        private void OnColorSelected(Color color)
        {
            Debug.Log($"Couleur s�lectionn�e : {color}");

            if (_equippedObjects.ContainsKey(_selectedSlotType))
            {
                GameObject equippedObject = _equippedObjects[_selectedSlotType];
                Renderer renderer = equippedObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // Dupliquer le mat�riau pour �viter de modifier le mat�riau partag�
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = color; // Appliquer la nouvelle couleur
                }
            }

            // D�sactiver les boutons d'action apr�s utilisation
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

            Debug.Log($"Nombre d'objets �quip�s : {_equippedObjects.Count}");

            // Parcourir tous les objets �quip�s et les afficher dans le ScrollView
            foreach (var equippedItem in _equippedObjects)
            {
                GameObject buttonObj = Instantiate(buttonPrefab, equippedItemsContent);
                if (buttonObj != null)
                {
                    var textMeshPro = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (textMeshPro != null)
                    {
                        textMeshPro.text = equippedItem.Value.name;
                        Debug.Log($"Bouton cr�� pour : {equippedItem.Value.name}");
                    }
                    else
                    {
                        Debug.LogWarning("TextMeshProUGUI non trouv� dans le bouton !");
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
                        Debug.LogWarning("Bouton non trouv� dans le prefab !");
                    }
                }
                else
                {
                    Debug.LogWarning("�chec de l'instanciation du bouton !");
                }
            }

            // D�sactiver les boutons d'action par d�faut
            buttonSupprimer.gameObject.SetActive(false);
            buttonChangeColor.gameObject.SetActive(false);
        }
        private void OnEquippedItemClicked(SlotType slotType)
        {
            // Stocker le slotType s�lectionn�
            _selectedSlotType = slotType;

            // Activer les boutons d'action
            buttonSupprimer.gameObject.SetActive(true);
            buttonChangeColor.gameObject.SetActive(true); // <-- Renommez buttonAutreAction en buttonChangeColor

            // Configurer les actions des boutons
            buttonSupprimer.onClick.RemoveAllListeners();
            buttonSupprimer.onClick.AddListener(() => UnequipPrefab(_selectedSlotType));

            buttonChangeColor.onClick.RemoveAllListeners(); // <-- Renommez buttonAutreAction en buttonChangeColor
            buttonChangeColor.onClick.AddListener(() => OnChangeColor(_selectedSlotType)); // <-- Appeler OnChangeColor

            Debug.Log($"Boutons d'action activ�s pour le slot : {slotType}");
        }
    }
}
