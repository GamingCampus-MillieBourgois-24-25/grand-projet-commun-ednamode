using CharacterCustomization;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// 
/// Stocke et applique les données de personnalisation d’un joueur réseau, de manière synchronisée.
/// 
public class PlayerCustomizationData : NetworkBehaviour
{
    [ContextMenu("🧪 LogTenue()")]
    public void LogTenue()
    {
        Debug.Log($"[CustomizationData] 🔍 Tenue du joueur {OwnerClientId}");

        foreach (var kvp in Data.equippedItemIds)
        {
            var slot = kvp.Key;
            var itemId = kvp.Value;

            Data.TryGetColor(slot, out var color);
            Data.TryGetTexture(slot, out var texture);

            Debug.Log($"→ {slot}: {itemId} | 🎨 {ColorUtility.ToHtmlStringRGBA(color)} | 🧵 {texture}");
        }
    }

    #region 📦 Données locales

    /// <summary>
    /// Données de personnalisation (locales mais synchronisées manuellement).
    /// </summary>
    public CustomizationData Data = new();

    #endregion

    #region 🎮 Application des visuels

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("[CustomizationData] ✨ Joueur local initialisé avec sa personnalisation.");
        }
    }

    public void ApplyToVisuals(EquippedVisualsHandler handler, List<Item> allItems)
    {
        if (handler == null)
        {
            Debug.LogWarning("[PlayerCustomizationData] ⚠ Aucun handler passé pour ApplyToVisuals.");
            return;
        }

        if (Data.equippedItemIds == null || Data.equippedItemIds.Count == 0)
        {
            Debug.LogWarning($"[ApplyToVisuals] ⚠ Aucune donnée de tenue pour le joueur {OwnerClientId}.");
            return;
        }

        foreach (var kvp in Data.equippedItemIds)
        {
            SlotType slot = kvp.Key;
            string itemId = kvp.Value;

            var item = allItems.FirstOrDefault(i => i.itemId == itemId);
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning($"[ApplyToVisuals] ⚠ itemId vide pour {slot} sur joueur {OwnerClientId}");
                continue;
            }
            if (item == null || item.prefab == null)
            {
                Debug.LogWarning($"[ApplyToVisuals] ❌ Item invalide pour {slot} → {itemId}");
                continue;
            }

            Data.TryGetColor(slot, out var color);
            Data.TryGetTexture(slot, out var textureName);

            handler.Equip(slot, item.prefab, color, textureName);
        }
    }

    public void SetItemAndApplyLocal(SlotType slotType, string itemId, Item item)
    {
        if (!IsSpawned || NetworkObject == null)
        {
            Debug.LogWarning("[CustomizationData] ❌ NetworkObject non prêt — annulation");
            return;
        }

        Debug.Log($"[CustomizationData] 🎯 Application locale de {itemId} dans {slotType}");

        Data.SetItem(slotType, itemId);

        var handler = GetComponentInChildren<EquippedVisualsHandler>();
        if (handler != null)
        {
            handler.Equip(slotType, item.prefab);
        }
        else
        {
            Debug.LogWarning("[CustomizationData] ⚠ Aucun EquippedVisualsHandler trouvé.");
        }

        SyncCustomizationDataServerRpc(Data);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncCustomizationDataServerRpc(CustomizationData data)
    {
        Data = data;
        UpdateVisualsOnAllClientsClientRpc(data);
    }

    [ClientRpc]
    private void UpdateVisualsOnAllClientsClientRpc(CustomizationData data)
    {
        // ❄ï¸ Bonus sécurité : ne ré-applique pas au joueur qui est le Host (qui a déjà la tenue localement)
        if (IsHost && IsOwner)
        {
            Debug.Log("[CustomizationData] ❄ï¸ Host local - pas de réapplication forçée.");
            return;
        }

        Data = data;
        var handler = GetComponentInChildren<EquippedVisualsHandler>(true);
        if (handler == null)
        {
            Debug.LogWarning("[CustomizationData] ⚠ Pas de handler pour appliquer les visuels.");
            return;
        }

        List<Item> allItems = Resources.LoadAll<Item>("Items").ToList();
        ApplyToVisuals(handler, allItems);
    }
}

#endregion