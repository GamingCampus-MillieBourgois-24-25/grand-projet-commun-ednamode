// ?? RunwayManager : Orchestration des défilés joueur par joueur
// Gère le cycle du défilé, déclenche l'UI (RunwayUIManager), les votes, le timing, la caméra, etc.

using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Netcode.Components;

public class RunwayManager : NetworkBehaviour
{
    #region ?? Références

    public static RunwayManager Instance { get; private set; }

    [Header("🎥 Défilé")]
    [Tooltip("Durée d'un passage de défilé par joueur (vote inclus)")]
    [SerializeField] private float runwayDurationPerPlayer = 7f;

    [Header("Effets")]
    [Tooltip("SFX à jouer pour annoncer un joueur")]
    [SerializeField] private AudioClip runwayAnnounceSFX;

    [Tooltip("AudioSource utilisée pour jouer les effets sonores")]
    [SerializeField] private AudioSource sfxAudioSource;

    #endregion

    #region Cycle de défilé

    private List<ulong> orderedPlayers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Débute la séquence de défilé + vote pour tous les joueurs connectés
    /// </summary>
    public void StartRunwayPhase()
    {
        if (!IsServer) return;

        Debug.Log("[Runway] 🚀 Début de la phase de défilé !");

        orderedPlayers = NetworkManager.Singleton.ConnectedClientsList
            .Select(c => c.ClientId)
            .OrderBy(id => id)
            .ToList();

        StartCoroutine(RunwaySequenceCoroutine());
    }

    private IEnumerator RunwaySequenceCoroutine()
    {
        foreach (var clientId in orderedPlayers)
        {
            AskClientToTeleport(clientId); // Téléportation du joueur sur le runway
            StartRunwayForClientRpc(clientId);  // UI + caméra locale
            yield return new WaitForSeconds(runwayDurationPerPlayer);
            EndRunwayForClientRpc(clientId);
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion

    #region Déclenchements UI côté clients

    [ClientRpc]
    private void StartRunwayForClientRpc(ulong clientId)
    {
        if (!IsClient) return;

        RunwayUIManager.Instance?.ShowCurrentRunwayPlayer(clientId);
        

        var targetPlayer = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (targetPlayer != null)
        {
            FindObjectOfType<RunwayCameraController>()?.StartPhotoSequence(targetPlayer.transform);
        }

        PlayIntroSFX();
    }

    [ClientRpc]
    private void EndRunwayForClientRpc(ulong clientId)
    {
        if (!IsClient) return;
        RunwayUIManager.Instance?.HideRunwayPanel();
    }

    #endregion

    #region Teleportation

    private void TeleportPlayerToRunway(ulong clientId)
    {
        Debug.Log($"[Runway] Tentative de téléportation du joueur {clientId}");

        var player = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (player == null)
        {
            Debug.LogWarning($"[Runway] ❌ Joueur {clientId} introuvable pour téléportation.");
            return;
        }

        Transform runwaySpot = GameObject.Find("RunwaySpot")?.transform;
        if (runwaySpot == null)
        {
            Debug.LogError("[Runway] 🚫 Aucun RunwaySpot trouvé !");
            return;
        }

        var netTransform = player.GetComponent<NetworkTransform>();
        netTransform.Teleport(runwaySpot.position, runwaySpot.rotation, player.transform.localScale);

        Debug.Log($"[Runway] 🚶 Joueur {clientId} téléporté !");
    }

    [ClientRpc]
    private void TeleportClientRpc(ulong targetClientId, ulong executingClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != executingClientId)
            return; // Ce RPC est uniquement pour le client concerné

        var player = NetworkPlayerManager.Instance.GetLocalPlayer();
        if (player == null)
        {
            Debug.LogWarning("[Runway] 🚫 Joueur local introuvable pour téléportation !");
            return;
        }

        Transform runwaySpot = GameObject.Find("RunwaySpot")?.transform;
        if (runwaySpot == null)
        {
            Debug.LogError("[Runway] 🚫 Aucun RunwaySpot trouvé !");
            return;
        }

        var netTransform = player.GetComponent<NetworkTransform>();
        netTransform.Teleport(runwaySpot.position, runwaySpot.rotation, player.transform.localScale);

        Debug.Log($"[Runway] ✅ Joueur local téléporté pour défiler !");
    }

    [ServerRpc(RequireOwnership = false)]
    private void TeleportPlayerToRunwayServerRpc(ulong clientId)
    {
        TeleportPlayerToRunway(clientId);
    }

    private void AskClientToTeleport(ulong clientId)
    {
        TeleportClientRpc(clientId, clientId);
    }

    #endregion

    #region Effets sonores

    private void PlayIntroSFX()
    {
        if (runwayAnnounceSFX != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(runwayAnnounceSFX);
        }
    }

    private void PlayOutroSFX()
    {
        if (runwayAnnounceSFX != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(runwayAnnounceSFX);
        }
    }

    #endregion

    #region Utilitaires

    public float GetRunwayDuration() => runwayDurationPerPlayer;

    #endregion
}
