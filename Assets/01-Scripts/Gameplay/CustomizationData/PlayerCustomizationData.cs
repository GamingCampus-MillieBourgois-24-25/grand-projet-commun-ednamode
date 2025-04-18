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
    [ContextMenu("🧪 LogTenue()")]
    public void LogTenue()
    {
        Debug.Log($"[CustomizationData] 🔍 Tenue du joueur {OwnerClientId}");

        foreach (var kvp in Data.Value.equippedItemIds)
        {
            var slot = kvp.Key;
            var itemId = kvp.Value;

            Data.Value.TryGetColor(slot, out var color);
            Data.Value.TryGetTexture(slot, out var texture);

            Debug.Log($"→ {slot}: {itemId} | 🎨 {ColorUtility.ToHtmlStringRGBA(color)} | 🧵 {texture}");
        }
    }


    #region 📦 Données synchronisées

    /// <summary>
    /// Données de personnalisation (équipements) synchronisées via Netcode.
    /// </summary>
    public NetworkVariable<CustomizationData> Data =
        new(writePerm: NetworkVariableWritePermission.Owner);
    #endregion

    #region 🎮 Application des visuels

    /// <summary>
    /// Applique les objets équipés à un handler visuel, basé sur les données synchronisées.
    /// ⚠ Doit être appelé uniquement quand tous les objets sont prêts.
    /// </summary>
    public void ApplyToVisuals(EquippedVisualsHandler handler, List<Item> allItems)
    {
        if (handler == null)
        {
            Debug.LogWarning("[PlayerCustomizationData] ⚠ Aucun handler passé pour ApplyToVisuals.");
            return;
        }

        if (Data.Value.equippedItemIds == null || Data.Value.equippedItemIds.Count == 0)
        {
            Debug.LogWarning($"[ApplyToVisuals] ⚠ Aucune donnée de tenue pour le joueur {OwnerClientId}.");
            return;
        }

        foreach (var kvp in Data.Value.equippedItemIds)
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

            // Ajout : récupérer couleur + texture
            Data.Value.TryGetColor(slot, out var color);
            Data.Value.TryGetTexture(slot, out var textureName);

            handler.Equip(slot, item.prefab, color, textureName);
        }
    }

    /// <summary>
    /// Méthode appelée localement pour appliquer une tenue à ce joueur, sans attendre de validation réseau.
    /// </summary>
    public void SetItemAndApplyLocal(SlotType slotType, string itemId, Item item)
    {
        if (!IsSpawned || NetworkObject == null)
        {
            Debug.LogWarning("[CustomizationData] ❌ NetworkObject non prêt — annulation");
            return;
        }

        Debug.Log($"[CustomizationData] 🎯 Application locale de {itemId} dans {slotType}");

        Data.Value.SetItem(slotType, itemId);

        var handler = GetComponentInChildren<EquippedVisualsHandler>();
        if (handler != null)
        {
            handler.Equip(slotType, item.prefab);
        }
        else
        {
            Debug.LogWarning("[CustomizationData] ⚠ Aucun EquippedVisualsHandler trouvé.");
        }

        RequestEquipItemServerRpc(slotType, itemId);
    }

    /// <summary>
    /// Requête envoyée par le client au serveur pour synchroniser l’équipement sélectionné.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestEquipItemServerRpc(SlotType slotType, string itemId)
    {
        Debug.Log($"[PlayerCustomizationData] 🛰️ Equipement demandé : {slotType} → {itemId}");
        Data.Value.SetItem(slotType, itemId);
    }

    /// <summary>
    /// Requête envoyée par le client au serveur pour rafraîchir les visuels.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SendRefreshServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[CustomizationData] 🔁 Requête de refresh reçue de {OwnerClientId}");

        var handler = GetComponentInChildren<EquippedVisualsHandler>(true);
        if (handler == null)
        {
            Debug.LogWarning("[CustomizationData] ⚠ Pas de visuals handler pour appliquer");
            return;
        }

        List<Item> allItems = Resources.LoadAll<Item>("Items").ToList();
        ApplyToVisuals(handler, allItems);
    }


    #endregion
}