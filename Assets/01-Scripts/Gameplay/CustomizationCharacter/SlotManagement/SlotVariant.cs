using UnityEngine;

namespace CharacterCustomization
{
    public class SlotVariant
    {
        public Item Item { get; } 
        public GameObject Prefab => Item.prefab; 
        public GameObject PreviewObject { get; }
        public string Name => Prefab.name;

        public SlotVariant(Item item)
        {
            Item = item;
            PreviewObject = PreviewCreator.CreateVariantPreview(item.prefab);
        }
    }
}