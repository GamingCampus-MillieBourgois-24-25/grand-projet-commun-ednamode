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
    private static List<Item> _cachedItems;

    /// <summary>
    /// Charge et met en cache la liste des items pour éviter de multiples appels à Resources.LoadAll.
    /// </summary>
    private static List<Item> GetCachedItems()
    {
        if (_cachedItems == null)
        {
            _cachedItems = Resources.LoadAll<Item>("Items").ToList();
            Debug.Log($"[PlayerCustomizationData] 🔍 {(_cachedItems.Count > 0 ? $"Chargé {_cachedItems.Count} items" : "⚠ Aucun item chargé")}");
        }
        return _cachedItems;
    }

    [ContextMenu("🧪 LogTenue()")]
    public void LogTenue()
    {
        Debug.Log($"[CustomizationData] 🔍 Tenue du joueur {OwnerClientId}");
        if (Data.Value.equippedItemIds == null) return;

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
        new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerCustomizationData] 🚀 OnNetworkSpawn pour joueur {OwnerClientId} (IsServer: {IsServer}, IsClient: {IsClient})");

        if (IsServer || IsClient)
        {
            Data.OnValueChanged += OnCustomizationDataChanged;
        }

        // Appliquer les visuels pour ce joueur
        ApplyVisualsForPlayer();

        // Si c'est un client ou l'hôte, mettre à jour tous les visuels
        if (IsClient || IsServer)
        {
            UpdateAllPlayersVisuals();
        }

        // Si c'est le serveur, forcer une synchronisation initiale pour tous les clients
        if (IsServer)
        {
            RefreshAllClientsClientRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer || IsClient)
        {
            Data.OnValueChanged -= OnCustomizationDataChanged;
        }
    }

    #endregion

    #region 🎮 Application des visuels

    /// <summary>
    /// Applique les visuels pour ce joueur spécifique.
    /// </summary>
    private void ApplyVisualsForPlayer()
    {
        if (Data.Value.equippedItemIds == null || Data.Value.equippedItemIds.Count == 0)
        {
            Debug.LogWarning($"[PlayerCustomizationData] ⚠ Aucune donnée de tenue pour le joueur {OwnerClientId}");
            return;
        }

        Debug.Log($"[PlayerCustomizationData] 🧠 Application des visuels pour le joueur {OwnerClientId}");

        var handler = GetComponentInChildren<EquippedVisualsHandler>(true);
        if (handler != null)
        {
            ApplyToVisuals(handler, GetCachedItems());
        }
        else
        {
            Debug.LogError($"[PlayerCustomizationData] ❌ Aucun EquippedVisualsHandler trouvé pour le joueur {OwnerClientId}");
        }
    }

    /// <summary>
    /// Met à jour les visuels de tous les joueurs dans la scène sur ce client.
    /// </summary>
    private void UpdateAllPlayersVisuals()
    {
        Debug.Log($"[PlayerCustomizationData] 🔄 Mise à jour des visuels pour tous les joueurs sur le client {NetworkManager.Singleton.LocalClientId}");

        var allPlayers = FindObjectsOfType<PlayerCustomizationData>();
        Debug.Log($"[PlayerCustomizationData] 🔍 {allPlayers.Length} joueurs trouvés dans la scène");

        foreach (var player in allPlayers)
        {
            if (player.Data.Value.equippedItemIds == null || player.Data.Value.equippedItemIds.Count == 0)
            {
                Debug.LogWarning($"[PlayerCustomizationData] ⚠ Aucune donnée de tenue pour le joueur {player.OwnerClientId}");
                continue;
            }

            Debug.Log($"[PlayerCustomizationData] 🎨 Application des visuels pour le joueur {player.OwnerClientId}");

            var handler = player.GetComponentInChildren<EquippedVisualsHandler>(true);
            if (handler != null)
            {
                player.ApplyToVisuals(handler, GetCachedItems());
            }
            else
            {
                Debug.LogError($"[PlayerCustomizationData] ❌ Aucun EquippedVisualsHandler trouvé pour le joueur {player.OwnerClientId}");
            }
        }
    }

    /// <summary>
    /// Applique les objets équipés à un handler visuel, basé sur les données synchronisées.
    /// </summary>
    public void ApplyToVisuals(EquippedVisualsHandler handler, List<Item> allItems)
    {
        if (handler == null)
        {
            Debug.LogError("[PlayerCustomizationData] ⚠ Handler null dans ApplyToVisuals.");
            return;
        }

        if (Data.Value.equippedItemIds == null || Data.Value.equippedItemIds.Count == 0)
        {
            Debug.LogWarning($"[ApplyToVisuals] ⚠ Aucune donnée de tenue pour le joueur {OwnerClientId}.");
            return;
        }

        Debug.Log($"[ApplyToVisuals] 🔄 Application des visuels pour {OwnerClientId} avec {Data.Value.equippedItemIds.Count} items");

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

            Data.Value.TryGetColor(slot, out var color);
            Data.Value.TryGetTexture(slot, out var textureName);

            Debug.Log($"[ApplyToVisuals] 🎯 Équipement de {slot}: {itemId} (Couleur: {ColorUtility.ToHtmlStringRGBA(color)}, Texture: {textureName})");

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

        Debug.Log($"[CustomizationData] 🎯 Application locale de {itemId} dans {slotType} pour {OwnerClientId}");

        Data.Value.SetItem(slotType, itemId);

        var handler = GetComponentInChildren<EquippedVisualsHandler>();
        if (handler != null)
        {
            handler.Equip(slotType, item.prefab);
        }
        else
        {
            Debug.LogError("[CustomizationData] ⚠ Aucun EquippedVisualsHandler trouvé.");
        }

        RequestEquipItemServerRpc(slotType, itemId);
    }

    /// <summary>
    /// Requête envoyée par le client au serveur pour synchroniser l’équipement sélectionné.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestEquipItemServerRpc(SlotType slotType, string itemId)
    {
        Debug.Log($"[PlayerCustomizationData] 🛰️ Équipement demandé : {slotType} → {itemId} pour le joueur {OwnerClientId}");

        var allItems = GetCachedItems();
        var item = allItems.FirstOrDefault(i => i.itemId == itemId);
        if (item == null)
        {
            Debug.LogWarning($"[PlayerCustomizationData] ⚠ Item {itemId} non trouvé pour {slotType}");
            return;
        }

        Data.Value.SetItem(slotType, itemId);

        var handler = GetComponentInChildren<EquippedVisualsHandler>(true);
        if (handler != null)
        {
            ApplyToVisuals(handler, allItems);
        }
        else
        {
            Debug.LogError($"[PlayerCustomizationData] ❌ Aucun EquippedVisualsHandler trouvé pour appliquer l'équipement sur le serveur pour {OwnerClientId}");
        }

        RefreshAllClientsClientRpc();
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

        ApplyToVisuals(handler, GetCachedItems());
        RefreshAllClientsClientRpc();
    }

    /// <summary>
    /// Notifie tous les clients de mettre à jour les visuels de tous les joueurs.
    /// </summary>
    [ClientRpc]
    private void RefreshAllClientsClientRpc()
    {
        Debug.Log($"[PlayerCustomizationData] 🔄 Rafraîchissement des visuels pour tous les joueurs sur le client {NetworkManager.Singleton.LocalClientId}");
        UpdateAllPlayersVisuals();
    }

    private void OnCustomizationDataChanged(CustomizationData previousValue, CustomizationData newValue)
    {
        Debug.Log($"[PlayerCustomizationData] 🔄 Données de personnalisation modifiées pour le joueur {OwnerClientId} sur client {NetworkManager.Singleton.LocalClientId}");

        ApplyVisualsForPlayer();

        if (IsServer)
        {
            Debug.Log($"[PlayerCustomizationData] 🔔 Serveur notifie tous les clients pour le joueur {OwnerClientId}");
            RefreshAllClientsClientRpc();
        }
    }

    #endregion
}