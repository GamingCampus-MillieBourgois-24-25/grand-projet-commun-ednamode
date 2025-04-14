using Unity.Netcode;
using UnityEngine;
using CharacterCustomization;
using System.Collections.Generic;
using System.Linq;

public class PlayerCustomizationData : NetworkBehaviour
{
    public NetworkVariable<CustomizationData> Data = new(writePerm: NetworkVariableWritePermission.Owner);

    public void ApplyToVisuals(EquippedVisualsHandler handler, List<Item> allItems)
    {
        foreach (var kvp in Data.Value.equippedItemIds)
        {
            SlotType slot = kvp.Key;
            int itemId = kvp.Value;
            var item = allItems.FirstOrDefault(i => i.GetInstanceID() == itemId && i.category == slot);
            if (item != null)
            {
                handler.Equip(slot, item.prefab);
            }
        }
    }
}