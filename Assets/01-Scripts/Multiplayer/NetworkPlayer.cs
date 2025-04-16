using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;

/// <summary>
/// Préfab réseau du joueur avec caméra locale et synchronisation de position.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer : NetworkBehaviour
{
    [Header("🎥 Caméra de customisation locale")]
    [SerializeField] private Camera customizationCamera;
    [SerializeField] private Vector3 cameraOffset = new(0f, 2f, -4f);

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (customizationCamera != null)
            customizationCamera.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            StartCoroutine(DelayedCameraActivation());
        }

        if (IsServer)
        {
            TeleportToSpawnPoint();
        }
        else
        {
            if (customizationCamera != null)
                customizationCamera.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkPlayerManager.Instance?.ReleaseSpawnPoint(OwnerClientId);
        }
    }

    /// <summary>
    /// Téléporte le joueur à son point de spawn assigné.
    /// </summary>
    private void TeleportToSpawnPoint()
    {
        var spawn = NetworkPlayerManager.Instance?.GetSpawnPoint(OwnerClientId);
        if (spawn != null)
        {
            transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            Debug.Log($"[NetworkPlayer] 🧍 Joueur {OwnerClientId} téléporté au point {spawn.name}");
        }
        else
        {
            Debug.LogWarning("[NetworkPlayer] ⚠ Aucun point de spawn trouvé pour ce client.");
        }
    }

    /// <summary>
    /// Active la caméra locale pour le joueur propriétaire, après un court délai de synchronisation.
    /// </summary>
    private IEnumerator DelayedCameraActivation()
    {
        yield return new WaitForSeconds(0.2f); // Laisse le temps au NetworkTransform de synchroniser la position
        ActivateCamera();
    }

    /// <summary>
    /// Active la caméra locale pour le joueur propriétaire.
    /// </summary>
    private void ActivateCamera()
    {
        if (customizationCamera == null) return;

        customizationCamera.enabled = true;
        customizationCamera.transform.position = transform.position + cameraOffset;
        customizationCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
        Debug.Log("[NetworkPlayer] 📷 Caméra locale activée.");
    }
}