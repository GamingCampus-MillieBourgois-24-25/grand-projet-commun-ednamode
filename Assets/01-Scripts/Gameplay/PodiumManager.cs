using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode.Components;

public class PodiumManager : NetworkBehaviour
{
    [Header("🎖️ Emplacements du Podium")]
    [SerializeField] private Transform podiumSpot1;
    [SerializeField] private Transform podiumSpot2;
    [SerializeField] private Transform podiumSpot3;

    [Header("🎥 Caméra Podium")]
    [SerializeField] private Transform podiumCameraSpot;

    [Header("⏱️ Temps d'affichage du podium")]
    [SerializeField] private float podiumDisplayDuration = 8f;

    public static PodiumManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void StartPodiumSequence()
    {
        if (!IsServer) return;

        var topPlayers = VotingManager.Instance.GetRankedResults().Take(3).ToList();

        if (topPlayers.Count == 0)
        {
            Debug.LogWarning("[Podium] Aucun joueur à afficher !");
            return;
        }

        if (topPlayers.Count > 0) TeleportPlayerToSpot(topPlayers[0].clientId, podiumSpot1);
        if (topPlayers.Count > 1) TeleportPlayerToSpot(topPlayers[1].clientId, podiumSpot2);
        if (topPlayers.Count > 2) TeleportPlayerToSpot(topPlayers[2].clientId, podiumSpot3);

        // 🎥 Positionne la caméra pour tous
        SetPodiumCameraClientRpc();

        // 🖥️ Affiche le classement UI
        PodiumUIManager.Instance?.ShowRanking(topPlayers);

        StartCoroutine(ReturnAllPlayersToSpawnAfterDelay());
        PodiumUIManager.Instance.HideRanking();
    }

    private void TeleportPlayerToSpot(ulong clientId, Transform spot)
    {
        var player = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (player == null || spot == null)
        {
            Debug.LogWarning($"[Podium] Impossible de téléporter le joueur {clientId}");
            return;
        }

        var netTransform = player.GetComponent<NetworkTransform>();
        netTransform.Teleport(spot.position, spot.rotation, player.transform.localScale);

        Debug.Log($"[Podium] Joueur {clientId} déplacé sur le podium.");
    }

    [ClientRpc]
    private void SetPodiumCameraClientRpc()
    {
        var cam = NetworkPlayerManager.Instance.GetLocalPlayer()?.GetLocalCamera();
        if (cam != null && podiumCameraSpot != null)
        {
            cam.transform.position = podiumCameraSpot.position;
            cam.transform.rotation = podiumCameraSpot.rotation;
            cam.fieldOfView = 50f;  // Zoom léger pour effet "célébration"
            Debug.Log("[Podium] 🎥 Caméra positionnée pour le podium.");
        }
    }


    private IEnumerator ReturnAllPlayersToSpawnAfterDelay()
    {
        yield return new WaitForSeconds(podiumDisplayDuration);

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();
            if (player == null) continue;

            Transform spawn = NetworkPlayerManager.Instance.GetSpawnPoint(client.ClientId);
            var netTransform = player.GetComponent<NetworkTransform>();
            netTransform.Teleport(spawn.position, spawn.rotation, player.transform.localScale);
        }

        ResetAllCamerasClientRpc();
        Debug.Log("[Podium] Tous les joueurs ont été replacés à leur spawn.");
    }

    [ClientRpc]
    private void ResetAllCamerasClientRpc()
    {
        var cam = NetworkPlayerManager.Instance.GetLocalPlayer()?.GetLocalCamera();
        if (cam != null)
        {
            cam.transform.localPosition = Vector3.zero;  // Ou ta position par défaut
            cam.fieldOfView = 60f;
            Debug.Log("[Podium] 🎥 Caméra locale réinitialisée.");
        }
    }
}
