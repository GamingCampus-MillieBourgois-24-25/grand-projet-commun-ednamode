using System.Collections.Generic;
using UnityEngine;
namespace CharacterCustomization
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
    public class Item : ScriptableObject
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
