using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Singleton en scène pour détecter et référencer le joueur local.
/// Utilisé par l’UI ou les managers non réseau.
/// </summary>
public class NetworkPlayerManager : MonoBehaviour
{
    [Header("🎮 Gestionnaire de joueur réseau")]
    [Tooltip("Liste des 8 points de spawn pour les joueurs")]
    public List<Transform> spawnPoints = new(); // Liste des points de spawn disponibles
    private readonly Dictionary<ulong, int> occupiedSpawns = new(); // Dictionnaire pour suivre les spawns occupés

    public static NetworkPlayerManager Instance { get; private set; } // Singleton

    public PlayerCustomizationData LocalPlayerData { get; private set; } // Référence au joueur local

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        TryFindLocalPlayer();
    }

    private void Update()
    {
        if (LocalPlayerData == null)
            TryFindLocalPlayer();
    }

    /// <summary>
    /// Essaie de trouver le joueur local dans la scène
    /// </summary>
    public NetworkPlayer GetNetworkPlayerFrom(ulong clientId)
    {
        var allPlayers = FindObjectsOfType<NetworkPlayer>(true);
        return allPlayers.FirstOrDefault(p => p.OwnerClientId == clientId);
    }


    /// <summary>
    /// Essaie de trouver le joueur local dans la scène
    /// </summary>
    private void TryFindLocalPlayer()
    {
        var all = FindObjectsOfType<PlayerCustomizationData>();
        LocalPlayerData = all.FirstOrDefault(p => p.IsOwner);

        if (LocalPlayerData != null)
            Debug.Log("[NetworkPlayerManager] 🎮 Joueur local détecté.");
    }

    /// <summary>
    /// Retourne le Transform racine du corps du joueur local
    /// </summary>
    public Transform GetBodyRoot()
    {
        var handler = LocalPlayerData?.GetComponentInChildren<EquippedVisualsHandler>();
        if (handler == null) return null;

        var meshName = handler.GetTargetMeshName();
        var meshTransform = handler.transform.Find(meshName);

        if (meshTransform == null)
        {
            Debug.LogWarning($"[NetworkPlayerManager] ⚠ Aucun Transform nommé '{meshName}' trouvé dans le joueur local.");
        }

        return meshTransform;
    }

    /// <summary>
    /// Retourne le handler de visuels équipés du joueur local
    /// </summary>
    public EquippedVisualsHandler GetLocalVisuals()
    {
        return LocalPlayerData?.GetComponentInChildren<EquippedVisualsHandler>();
    }

    /// <summary>
    /// Retourne le joueur local
    /// </summary>
    public NetworkPlayer GetLocalPlayer()
    {
        return FindObjectsOfType<NetworkPlayer>().FirstOrDefault(p => p.IsOwner);
    }


    /// <summary>
    /// Retourne le Transform associé à un clientId donné
    /// </summary>
    public Transform GetSpawnPoint(ulong clientId)
    {
        if (occupiedSpawns.ContainsKey(clientId))
            return spawnPoints[occupiedSpawns[clientId]];

        int index = GetAvailableIndex();
        occupiedSpawns[clientId] = index;
        return spawnPoints[index];
    }

    /// <summary>
    /// Retourne le Transform associé à un clientId donné
    /// </summary>
    private int GetAvailableIndex()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
            if (!occupiedSpawns.ContainsValue(i))
                return i;

        return 0; // fallback
    }

    /// <summary>
    /// Libère le point de spawn pour un clientId donné
    /// </summary>
    public void ReleaseSpawnPoint(ulong clientId)
    {
        if (occupiedSpawns.ContainsKey(clientId))
        {
            Debug.Log($"[NetworkPlayerManager] 🔓 Libération du spawn pour client {clientId}");
            occupiedSpawns.Remove(clientId);
        }
    }

}
