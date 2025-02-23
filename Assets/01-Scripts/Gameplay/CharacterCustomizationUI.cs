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
        public SlotLibrary slotLibrary; // Ajoutez ce champ pour la bibliothèque de slots

        private Dictionary<SlotType, GameObject> _equippedObjects = new Dictionary<SlotType, GameObject>();
        public void Initialize(CharacterCustomization character)
        {
            _characterGameObject = new CharacterCustomization(characterPrefab, slotLibrary);

            // Accéder au GameObject du personnage
            GameObject characterInstance = _characterGameObject.CharacterInstance;
            Debug.Log($"Personnage instancié : {characterInstance.name}");

            PopulateUI();
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

                var meshes = slot.GetAvailableMeshes();
                Debug.Log($"Il y a {meshes.Count()} meshes disponibles pour le slot {slotUI.slotType}");

                foreach (var meshOption in meshes)
                {
                    GameObject buttonObj = Instantiate(buttonPrefab, slotUI.content);
                    buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = meshOption.name;
                    Button button = buttonObj.GetComponent<Button>();

                    button.onClick.AddListener(() => EquipMesh(slotUI.slotType, meshOption));
                }
            }
        }


        private void EquipMesh(SlotType slotType, Mesh mesh)
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
                        // Vérifier si le mesh sélectionné est le même que celui déjà équipé
                        var meshFilter = previousObject.GetComponentInChildren<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh == mesh)
                        {
                            // Réactiver l'objet précédent
                            previousObject.SetActive(true);
                            Debug.Log($"Réactivation de l'objet précédent pour {slotType}.");
                            return; // Ne pas créer un nouvel objet
                        }
                        else
                        {
                            // Désactiver l'objet précédent
                            previousObject.SetActive(false);
                            Debug.Log($"Ancien objet pour {slotType} désactivé.");
                        }
                    }
                }

                // Appliquer le nouveau mesh
                slot.SetMesh(mesh);
                slot.Toggle(true); // S'assurer que le slot est activé et visible

                // Utiliser slot.Preview pour obtenir l'objet cible
                GameObject targetObject = slot.Preview;
                if (targetObject != null)
                {
                    targetObject.transform.SetParent(_characterGameObject.CharacterInstance.transform);
                    targetObject.transform.localPosition = Vector3.zero; // Réinitialiser la position relative
                    targetObject.SetActive(true); // Activer le nouvel objet
                    Debug.Log($"Position des lunettes : {targetObject.transform.position}");
                    Debug.Log($"Parent des lunettes : {targetObject.transform.parent?.name ?? "NULL"}");

                    // Stocker la référence du nouvel objet équipé
                    _equippedObjects[slotType] = targetObject;
                }

                Debug.Log($"Mesh changé pour {slotType} : {mesh.name}");
                _characterGameObject.RefreshCustomization(); // Force la mise à jour visuelle
            }
            else
            {
                Debug.LogWarning($"Slot {slotType} non trouvé lors du changement de mesh !");
            }
        }
    }
}
