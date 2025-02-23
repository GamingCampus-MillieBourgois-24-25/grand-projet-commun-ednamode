using UnityEngine;

namespace CharacterCustomization
{
    public class SlotVariant
    {
        public GameObject Prefab { get; } // Prefab de l'objet
        public GameObject PreviewObject { get; } // Objet de pr�visualisation
        public string Name => Prefab.name; // Nom du prefab

        public SlotVariant(GameObject prefab)
        {
            Prefab = prefab;
            PreviewObject = PreviewCreator.CreateVariantPreview(prefab); // Cr�e une pr�visualisation du prefab
        }
    }
}