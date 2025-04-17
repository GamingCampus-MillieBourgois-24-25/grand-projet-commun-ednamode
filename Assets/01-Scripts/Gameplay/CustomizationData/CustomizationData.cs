using Unity.Netcode;
using System.Collections.Generic;
using CharacterCustomization;

[System.Serializable]
public struct CustomizationData : INetworkSerializable
{
    public Dictionary<SlotType, string> equippedItemIds;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsWriter)
        {
            int count = equippedItemIds?.Count ?? 0;
            serializer.SerializeValue(ref count);

            if (equippedItemIds != null)
            {
                foreach (var kvp in equippedItemIds)
                {
                    int key = (int)kvp.Key;
                    string val = kvp.Value;

                    serializer.SerializeValue(ref key);
                    serializer.SerializeValue(ref val);
                }
            }
        }
        else
        {
            int count = 0;
            serializer.SerializeValue(ref count);
            equippedItemIds = new();

            for (int i = 0; i < count; i++)
            {
                int key = 0;
                string val = string.Empty;

                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);

                equippedItemIds[(SlotType)key] = val;
            }
        }
    }


    public void SetItem(SlotType slotType, string itemId)
    {
        if (equippedItemIds == null)
            equippedItemIds = new();
        equippedItemIds[slotType] = itemId;
    }

    //public bool TryGetItem(SlotType slot, out int itemId)
    //{
    //    itemId = -1;
    //    return equippedItemIds != null && equippedItemIds.TryGetValue(slot, out itemId);
    //}
}