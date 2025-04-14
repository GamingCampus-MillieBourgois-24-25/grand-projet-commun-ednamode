using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterCustomizationManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject characterPrefab; // Prefab du personnage de base
        public CustomizableCharacterUI customizableCharacterUI; // Référence à l’UI

        void Start()
        {
            // Vérifier que les références sont assignées
            if (characterPrefab == null)
            {
                Debug.LogError("characterPrefab n’est pas assigné dans CharacterCustomizationManager !");
                return;
            }

            if (customizableCharacterUI == null)
            {
                Debug.LogError("customizableCharacterUI n’est pas assigné dans CharacterCustomizationManager !");
                return;
            }

            // Initialiser l’UI sans passer de CharacterCustomization
            customizableCharacterUI.Initialize();
        }
    }
}