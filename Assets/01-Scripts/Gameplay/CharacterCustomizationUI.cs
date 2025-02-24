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
        public GameObject CharacterInstance { get; private set; }
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

                    Debug.Log($"Position des lunettes : {instance.transform.position}");
                    Debug.Log($"Parent des lunettes : {instance.transform.parent?.name ?? "NULL"}");

                    // Stocker la r�f�rence du nouvel objet �quip�
                    _equippedObjects[slotType] = instance;
                }

                Debug.Log($"Prefab chang� pour {slotType} : {prefab.name}");
                _characterGameObject.RefreshCustomization(); // Force la mise � jour visuelle
            }
            else
            {
                Debug.LogWarning($"Slot {slotType} non trouv� lors du changement de prefab !");
            }
        }
    }
}
