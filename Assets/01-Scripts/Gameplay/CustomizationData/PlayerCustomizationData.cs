using CharacterCustomization;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stocke et applique les données de personnalisation d’un joueur réseau, de manière synchronisée.
/// </summary>
public class PlayerCustomizationData : NetworkBehaviour
{
    #region 📦 Données synchronisées

    /// <summary>
    /// Données de personnalisation (équipements) synchronisées via Netcode.
    /// </summary>
    public NetworkVariable<CustomizationData> Data = new(writePerm: NetworkVariableWritePermission.Owner);

    #endregion

    #region 🎮 Application des visuels

    [ServerRpc(RequireOwnership = false)]
    public void RequestEquipItemServerRpc(SlotType slotType, int itemId)
    {
        if (!IsServer) return;

        Debug.Log($"[PlayerCustomizationData] 🛰️ Equipement demandé : {slotType} → {itemId}");

        Data.Value.SetItem(slotType, itemId);

        var handler = GetComponentInChildren<EquippedVisualsHandler>();
        if (handler != null)
        {
            var allItems = Resources.LoadAll<Item>("Items").ToList();
            ApplyToVisuals(handler, allItems);
        }
    }


    /// <summary>
    /// Applique les objets équipés à un handler visuel, basé sur les données synchronisées.
    /// ⚠ Doit être appelé uniquement côté serveur pour éviter les doublons.
    /// </summary>
    public void ApplyToVisuals(EquippedVisualsHandler handler, List<Item> allItems)
    {
        if (!IsServer)
        {
            Debug.Log("[PlayerCustomizationData] ⛔ ApplyToVisuals() ignoré côté client.");
            return;
        }

        var alreadyEquipped = new HashSet<SlotType>();

        foreach (var kvp in Data.Value.equippedItemIds)
        {
            SlotType slot = kvp.Key;
            int itemId = kvp.Value;

            // ✅ Ne pas équiper plusieurs fois le même slot
            if (alreadyEquipped.Contains(slot))
                continue;

            var item = allItems.FirstOrDefault(i => i.GetInstanceID() == itemId && i.category == slot);
            if (item != null)
            {
                handler.Equip(slot, item.prefab);
                alreadyEquipped.Add(slot);
                Debug.Log($"[PlayerCustomizationData] 🎽 Equipement appliqué : {item.name} pour {slot}");
            }
            else
            {
                Debug.LogWarning($"[PlayerCustomizationData] ⚠ Aucun item trouvé pour slot {slot} avec ID {itemId}");
            }
        }
    }

    #endregion
}
