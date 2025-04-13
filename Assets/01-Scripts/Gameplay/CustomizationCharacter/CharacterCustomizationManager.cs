using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterCustomizationManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject characterPrefab; // Prefab du personnage de base
        public CustomizableCharacterUI customizableCharacterUI; // R�f�rence � l�UI

        void Start()
        {
            // V�rifier que les r�f�rences sont assign�es
            if (characterPrefab == null)
            {
                Debug.LogError("characterPrefab n�est pas assign� dans CharacterCustomizationManager !");
                return;
            }

            if (customizableCharacterUI == null)
            {
                Debug.LogError("customizableCharacterUI n�est pas assign� dans CharacterCustomizationManager !");
                return;
            }

            // Initialiser l�UI sans passer de CharacterCustomization
            customizableCharacterUI.Initialize();
        }
    }
}