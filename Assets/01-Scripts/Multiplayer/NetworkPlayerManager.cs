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
    public List<Transform> spawnPoints = new();

    public static NetworkPlayerManager Instance { get; private set; }

    public PlayerCustomizationData LocalPlayerData { get; private set; }

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

    private void TryFindLocalPlayer()
    {
        var all = FindObjectsOfType<PlayerCustomizationData>();
        LocalPlayerData = all.FirstOrDefault(p => p.IsOwner);

        if (LocalPlayerData != null)
            Debug.Log("[NetworkPlayerManager] 🎮 Joueur local détecté.");
    }

    public Transform GetBodyRoot()
    {
        return LocalPlayerData?.GetComponentInChildren<EquippedVisualsHandler>()?.transform;
    }

    public EquippedVisualsHandler GetLocalVisuals()
    {
        return LocalPlayerData?.GetComponentInChildren<EquippedVisualsHandler>();
    }

    /// <summary>
    /// Retourne le Transform associé à un clientId donné
    /// </summary>
    public Transform GetSpawnPoint(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return null;
        int index = (int)(clientId % (ulong)spawnPoints.Count);
        return spawnPoints[index];
    }
}
