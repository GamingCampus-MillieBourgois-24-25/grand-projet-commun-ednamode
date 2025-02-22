using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterCustomizationManager : MonoBehaviour
    {
        public SlotLibrary slotLibrary; 

        private CharacterCustomization _characterCustomization; 

        public CustomizableCharacterUI customizableCharacterUI;

        void Start()
        {
            if (slotLibrary == null)
            {
                Debug.LogError("Le SlotLibrary n'est pas assign�.");
                return;
            }

            _characterCustomization = new CharacterCustomization(slotLibrary);

            if (customizableCharacterUI != null)
            {
                customizableCharacterUI.Initialize(_characterCustomization);
            }
            else
            {
                Debug.LogError("L'UI de personnalisation n'est pas assign�e.");
            }
        }
    }
}
