using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    public class FullBodyVariant
    {
        public FullBodyElement[] Elements { get; }
        public GameObject PreviewObject { get; }
        public string Name => Elements.First().Prefab.name;

        public FullBodyVariant(FullBodyEntry fullBodyEntry)
        {
            Elements = fullBodyEntry.Slots
                .Where(s => s.Item != null && s.Item.prefab != null) 
                .Select(s => new FullBodyElement(s.Type, s.Item.prefab))
                .ToArray();

            PreviewObject = PreviewCreator.CreateVariantPreview(GetPreviewPrefab(Elements));
        }

        private static GameObject GetPreviewPrefab(FullBodyElement[] elements)
        {
            var element = elements.FirstOrDefault(e => e.Type == SlotType.Hat)
                          ?? elements.FirstOrDefault(e => e.Type == SlotType.Outerwear)
                          ?? elements.First();

            if (element == null || element.Prefab == null)
            {
                Debug.LogError("Erreur : aucun élément ou prefab valide trouvé.");
                return null;
            }

            return element.Prefab;
        }

        public class FullBodyElement
        {
            public SlotType Type { get; }
            public GameObject Prefab { get; }

            public FullBodyElement(SlotType type, GameObject prefab)
            {
                Type = type;
                Prefab = prefab;
            }
        }
    }
}