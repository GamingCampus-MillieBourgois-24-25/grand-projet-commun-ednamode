using System;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    [CreateAssetMenu(menuName = "Character Customization /Slot Library", fileName = "SlotLibrary")]
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
        public GameObject GameObject;
    }

    [Serializable]
    public class SlotEntry
    {
        public SlotType Type;
        public SlotGroupEntry[] Groups;

        public GameObject[] Prefabs => Groups.SelectMany(group => group.Variants).ToArray();
    }

    [Serializable]
    public class SlotGroupEntry
    {
        public GroupType Type;
        public GameObject[] Variants;
    }


}
