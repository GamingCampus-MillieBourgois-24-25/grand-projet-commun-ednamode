using System.Collections.Generic;
using UnityEngine;
namespace CharacterCustomization
{
    [CreateAssetMenu(fileName = "Item", menuName = "Customization/Item")]
    public class CustomizationItem : ScriptableObject
    {
        private const string _path = "Resources/Items/";
        public string itemName;
        public Sprite icon;
        public GameObject prefab;
        public int price;
        public SlotType category;
        public List<string> tags = new List<string>();
    }
}
