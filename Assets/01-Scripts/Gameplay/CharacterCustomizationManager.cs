using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterCustomizationManager : MonoBehaviour
    {
        public SlotLibrary slotLibrary; 
        public GameObject characterPrefab; 

        private CharacterCustomization _characterCustomization;

        public CustomizableCharacterUI customizableCharacterUI;

        void Start()
        {
            _characterCustomization = new CharacterCustomization(characterPrefab, slotLibrary);
            customizableCharacterUI.Initialize(_characterCustomization);

            if (customizableCharacterUI != null)
            {
                customizableCharacterUI.Initialize(_characterCustomization);
            }
        }
    }
}