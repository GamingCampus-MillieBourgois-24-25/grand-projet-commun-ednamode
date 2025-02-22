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

        public List<SlotUI> slotUIs; 
        public GameObject buttonPrefab; 
        private CharacterCustomization _characterGameObject; 


        public void Initialize(CharacterCustomization character)
        {
            Debug.Log("Initialize appelé avec " + character);
            _characterGameObject = character;
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
                slot.SetMesh(mesh); 
                Debug.Log($"Mesh changé pour {slotType} : {mesh.name}");
            }
        }
    }
}
