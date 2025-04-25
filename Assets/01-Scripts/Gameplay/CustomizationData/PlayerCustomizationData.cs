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

            Debug.Log($"→ {slot}: {itemId} | 🎨 {ColorUtility.ToHtmlStringRGBA(color)} | 🧵 {(string.IsNullOrEmpty(texture) ? "None" : texture)}");
        }
    }

    #region 📦 Données locales

    /// <summary>
    /// Données de personnalisation (locales mais synchronisées manuellement).
    /// </summary>
    public CustomizationData Data = new()
    {
        equippedItemIds = new Dictionary<SlotType, string>(),
        equippedColors = new Dictionary<SlotType, Color32>(),
        equippedTextures = new Dictionary<SlotType, string>()
    };

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

        Debug.Log($"[ApplyToVisuals] Application des visuels pour joueur {OwnerClientId}. Données actuelles :");
        foreach (var kvp in Data.equippedColors)
        {
            Debug.Log($"[ApplyToVisuals] Couleur pour {kvp.Key}: {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
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

            Color color = Color.white;
            if (Data.TryGetColor(slot, out var color32))
            {
                color = color32;
                Debug.Log($"[ApplyToVisuals] Couleur appliquée pour {slot}: {ColorUtility.ToHtmlStringRGBA(color)}");
            }
            else
            {
                Debug.LogWarning($"[ApplyToVisuals] Aucune couleur trouvée pour {slot}, utilisation de blanc par défaut.");
            }

            Data.TryGetTexture(slot, out var textureName);

            if (string.IsNullOrEmpty(textureName))
            {
                handler.ApplyColorWithoutTexture(slot, color);
                Debug.Log($"[ApplyToVisuals] Appliqué sans texture pour {slot} avec couleur {ColorUtility.ToHtmlStringRGBA(color)}");
            }
            else
            {
                handler.Equip(slot, item.prefab, color, textureName);
                Debug.Log($"[ApplyToVisuals] Appliqué avec texture {textureName} pour {slot} avec couleur {ColorUtility.ToHtmlStringRGBA(color)}");
            }
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
            Color color = Color.white;
            if (Data.TryGetColor(slotType, out var color32))
            {
                color = color32;
                Debug.Log($"[SetItemAndApplyLocal] Utilisation de la couleur existante pour {slotType}: {ColorUtility.ToHtmlStringRGBA(color)}");
            }
            handler.Equip(slotType, item.prefab, color, null);
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
        Debug.Log($"[SyncCustomizationDataServerRpc] Synchronisation des données pour joueur {OwnerClientId}.");
        foreach (var kvp in data.equippedColors)
        {
            Debug.Log($"[SyncCustomizationDataServerRpc] Couleur pour {kvp.Key}: {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
        }
        Data = data;
        UpdateVisualsOnAllClientsClientRpc(data);
    }

    [ClientRpc]
    private void UpdateVisualsOnAllClientsClientRpc(CustomizationData data)
    {
        if (IsHost && IsOwner)
        {
            Debug.Log("[CustomizationData] ❄️ Host local - pas de réapplication forçée.");
            return;
        }

        Debug.Log($"[UpdateVisualsOnAllClientsClientRpc] Mise à jour des visuels pour joueur {OwnerClientId}.");
        foreach (var kvp in data.equippedColors)
        {
            Debug.Log($"[UpdateVisualsOnAllClientsClientRpc] Couleur pour {kvp.Key}: {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
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