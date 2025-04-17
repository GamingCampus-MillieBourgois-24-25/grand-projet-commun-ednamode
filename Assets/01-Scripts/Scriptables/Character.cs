using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    [CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
    public class Character : ScriptableObject
    {
        private const string _path = "Resources/Character/";
        public string characterName;
        public GameObject bodyType;

        // Dictionnaire pour associer chaque SlotType à un item
        public Dictionary<SlotType, Item> slotToItem = new Dictionary<SlotType, Item>();

        public void SetItem(SlotType slotType, Item item)
        {
            if (slotToItem.ContainsKey(slotType))
            {
                slotToItem[slotType] = item;
            }
            else
            {
                slotToItem.Add(slotType, item);
            }
        }

        public Item GetItem(SlotType slotType)
        {
            if (slotToItem.TryGetValue(slotType, out var item))
            {
                return item;
            }
            return null; // Retourne null si aucun item n'est associé
        }
    }
}
