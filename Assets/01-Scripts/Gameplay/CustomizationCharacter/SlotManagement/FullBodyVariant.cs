using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    public class FullBodyVariant
    {
        public FullBodyElement[] Elements { get; }
        public GameObject PreviewObject { get; }
        public string Name => Elements.First().Prefab.name; // Utilisez le nom du prefab

        public FullBodyVariant(FullBodyEntry fullBodyEntry)
        {
            Elements = fullBodyEntry.Slots
                .Where(s => s.GameObject != null) // V�rifiez que le GameObject n'est pas null
                .Select(s => new FullBodyElement(s.Type, s.GameObject)) // Utilisez le GameObject directement
                .ToArray();

            // Cr�ez la pr�visualisation � partir du prefab
            PreviewObject = PreviewCreator.CreateVariantPreview(GetPreviewPrefab(Elements));
        }

        private static GameObject GetPreviewPrefab(FullBodyElement[] elements)
        {
            // S�lectionnez un prefab pour la pr�visualisation (par exemple, le premier �l�ment)
            var element = elements.FirstOrDefault(e => e.Type == SlotType.Hat)
                          ?? elements.FirstOrDefault(e => e.Type == SlotType.Outerwear)
                          ?? elements.First();

            if (element == null || element.Prefab == null)
            {
                Debug.LogError("Erreur : aucun �l�ment ou prefab valide trouv�.");
                return null;
            }

            return element.Prefab;
        }

        public class FullBodyElement
        {
            public SlotType Type { get; }
            public GameObject Prefab { get; } // Utilisez un prefab au lieu d'un mesh

            public FullBodyElement(SlotType type, GameObject prefab)
            {
                Type = type;
                Prefab = prefab;
            }
        }
    }
}