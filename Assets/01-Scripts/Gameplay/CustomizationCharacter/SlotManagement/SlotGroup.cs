using System.Linq;

namespace CharacterCustomization
{
    public class SlotGroup
    {
        public readonly GroupType Type;
        public readonly SlotVariant[] Variants;

        public SlotGroup(GroupType type, Item[] items)
        {
            Type = type;
            Variants = items.Select(item => new SlotVariant(item)).ToArray();
        }
    }
}