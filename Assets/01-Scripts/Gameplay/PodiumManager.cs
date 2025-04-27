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

        if (topPlayers.Count > 0)
            TeleportPlayerToSpot(topPlayers[0].clientId, podiumSpot1, "1st Place");
        if (topPlayers.Count > 1)
            TeleportPlayerToSpot(topPlayers[1].clientId, podiumSpot2, "2nd Place");
        if (topPlayers.Count > 2)
            TeleportPlayerToSpot(topPlayers[2].clientId, podiumSpot3, "3rd Place");

        SetPodiumCameraClientRpc();
        PodiumUIManager.Instance?.ShowRanking(topPlayers);
        StartCoroutine(ReturnAllPlayersToSpawnAfterDelay());
    }

    private void TeleportPlayerToSpot(ulong clientId, Transform spot, string rankDescription)
    {
        if (!IsServer) return;

        var player = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (player == null)
        {
            Debug.LogWarning($"[Podium] Impossible de trouver le joueur {clientId}");
            return;
        }

        if (spot == null)
        {
            Debug.LogError($"[Podium] Spot non assigné pour {rankDescription} (ClientId: {clientId})");
            return;
        }

        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };

        TeleportPlayerClientRpc(clientId, spot.position, spot.rotation, player.transform.localScale, rpcParams);
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong clientId, Vector3 position, Quaternion rotation, Vector3 scale, ClientRpcParams rpcParams)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        var player = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (player == null)
        {
            Debug.LogWarning($"[Podium] Joueur {clientId} introuvable sur le client local.");
            return;
        }

        var netTransform = player.GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(position, rotation, scale);
        }
        else
        {
            Debug.LogError($"[Podium] NetworkTransform manquant pour le joueur {clientId}");
        }
    }

    [ClientRpc]
    private void SetPodiumCameraClientRpc()
    {
        var cam = NetworkPlayerManager.Instance.GetLocalPlayer()?.GetLocalCamera();
        if (cam != null && podiumCameraSpot != null)
        {
            cam.transform.position = podiumCameraSpot.position;
            cam.transform.rotation = podiumCameraSpot.rotation;
            cam.fieldOfView = 50f;
        }
        else
        {
            Debug.LogWarning("[Podium] Caméra ou podiumCameraSpot non défini.");
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
            if (spawn == null)
            {
                Debug.LogWarning($"[Podium] Spawn point non trouvé pour ClientId {client.ClientId}");
                continue;
            }

            TeleportPlayerToSpot(client.ClientId, spawn, "Spawn Point");
        }

        ResetAllCamerasClientRpc();
    }

    [ClientRpc]
    private void ResetAllCamerasClientRpc()
    {
        var cam = NetworkPlayerManager.Instance.GetLocalPlayer()?.GetLocalCamera();
        if (cam != null)
        {
            cam.transform.localPosition = Vector3.zero;
            cam.fieldOfView = 60f;
        }
    }
}