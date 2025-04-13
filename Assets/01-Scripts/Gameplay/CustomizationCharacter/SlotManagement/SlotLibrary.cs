using System;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    [CreateAssetMenu(menuName = "Character Customization/Slot Library", fileName = "SlotLibrary")]
    public class SlotLibrary : ScriptableObject
    {
        public FullBodyEntry[] FullBodyCostumes;
        public SlotEntry[] Slots;
    }

    [Serializable]
    public class FullBodyEntry
    {
        public FullBodySlotEntry[] Slots;
    }

    [Serializable]
    public class FullBodySlotEntry
    {
        public SlotType Type;
        public Item Item;
    }

    [Serializable]
    public class SlotEntry
    {
        public SlotType Type;
        public SlotGroupEntry[] Groups;

        public Item[] Items => Groups.SelectMany(group => group.Items).ToArray();
    }

    [Serializable]
    public class SlotGroupEntry
    {
        public GroupType Type;
        public Item[] Items; 
    }
}