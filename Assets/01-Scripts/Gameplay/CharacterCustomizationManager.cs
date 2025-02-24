using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterCustomizationManager : MonoBehaviour
    {
        public SlotLibrary slotLibrary; // Bibliothèque de slots
        public GameObject characterPrefab; // Prefab du personnage

        private CharacterCustomization _characterCustomization;

        public CustomizableCharacterUI customizableCharacterUI;

        void Start()
        {
            if (slotLibrary == null)
            {
                Debug.LogError("Le SlotLibrary n'est pas assigné.");
                return;
            }

            if (characterPrefab == null)
            {
                Debug.LogError("Le prefab du personnage n'est pas assigné.");
                return;
            }

            // Créer une instance de CharacterCustomization avec le prefab du personnage et la bibliothèque de slots
            _characterCustomization = new CharacterCustomization(characterPrefab, slotLibrary);

            if (customizableCharacterUI != null)
            {
                customizableCharacterUI.Initialize(_characterCustomization);
            }
            else
            {
                Debug.LogError("L'UI de personnalisation n'est pas assignée.");
            }
        }
    }
}