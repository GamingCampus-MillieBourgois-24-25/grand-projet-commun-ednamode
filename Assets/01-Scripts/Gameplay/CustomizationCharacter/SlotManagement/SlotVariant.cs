using UnityEngine;

namespace CharacterCustomization
{
    public class SlotVariant
    {
        public GameObject Prefab { get; } // Prefab de l'objet
        public GameObject PreviewObject { get; } // Objet de prévisualisation
        public string Name => Prefab.name; // Nom du prefab

        public SlotVariant(GameObject prefab)
        {
            Prefab = prefab;
            PreviewObject = PreviewCreator.CreateVariantPreview(prefab); // Crée une prévisualisation du prefab
        }
    }
}