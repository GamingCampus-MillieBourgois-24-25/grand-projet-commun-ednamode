
using CharacterCustomization;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerCustomizationData : NetworkBehaviour
{
    [ContextMenu("🔍 LogTenue()")]
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

    public CustomizationData Data = new()
    {
        equippedItemIds = new Dictionary<SlotType, string>(),
        equippedColors = new Dictionary<SlotType, Color32>(),
        equippedTextures = new Dictionary<SlotType, string>()
    };

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("[CustomizationData] ✨ Joueur local initialisé avec sa personnalisation.");
        }
        Data.EnsureInitialized();
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

        // 🎨 Debug complet des couleurs avant d'appliquer les visuels
        if (Data.equippedColors == null)
        {
            Debug.LogError($"[ApplyToVisuals] 🚨 Data.equippedColors est NULL !");
        }
        else if (Data.equippedColors.Count == 0)
        {
            Debug.LogWarning($"[ApplyToVisuals] ⚠️ Data.equippedColors est VIDE !");
        }
        else
        {
            Debug.Log($"[ApplyToVisuals] 🎨 Data.equippedColors contient {Data.equippedColors.Count} entrées :");
            foreach (var kvp in Data.equippedColors)
            {
                Debug.Log($"[ApplyToVisuals] Slot: {kvp.Key}, Couleur: {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
            }
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

            Color? colorOverride = null;
            if (Data.TryGetColor(slot, out var color32))
            {
                colorOverride = color32;
                Debug.Log($"[ApplyToVisuals] Couleur personnalisée appliquée pour {slot}: {ColorUtility.ToHtmlStringRGBA(color32)}");
            }
            else
            {
                Debug.Log($"[ApplyToVisuals] Aucune couleur personnalisée pour {slot}, on laisse la couleur du prefab.");
            }

            Data.TryGetTexture(slot, out var textureName);

            handler.Equip(slot, item.prefab, colorOverride, textureName);
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
            Color? color = null;
            if (Data.TryGetColor(slotType, out var c)) color = c;
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

        data.EnsureInitialized();

        // ✅ Mise à jour SANS créer de nouveaux dictionnaires
        foreach (var kvp in data.equippedItemIds)
            Data.equippedItemIds[kvp.Key] = kvp.Value;

        foreach (var kvp in data.equippedColors)
            Data.equippedColors[kvp.Key] = kvp.Value;

        foreach (var kvp in data.equippedTextures)
            Data.equippedTextures[kvp.Key] = kvp.Value;

        // ✅ Plus de passage CustomizationData → évite le wipe !
        UpdateVisualsOnAllClientsClientRpc();
    }


    [ClientRpc]
    private void UpdateVisualsOnAllClientsClientRpc()
    {
        Debug.Log($"[ClientRpc] Re-applique les visuels pour joueur {OwnerClientId}");

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
