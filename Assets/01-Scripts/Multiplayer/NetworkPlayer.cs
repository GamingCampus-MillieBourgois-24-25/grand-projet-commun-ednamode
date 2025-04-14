using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Gère le joueur réseau instancié automatiquement par Netcode.
/// Active et positionne la caméra de customisation uniquement pour le joueur local.
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    [Header("🎥 Caméra locale de customisation")]
    [Tooltip("Caméra à activer uniquement pour le joueur local (doit être enfant du prefab)")]
    [SerializeField] private Camera playerCustomizationCamera;

    [Tooltip("Décalage fixe de la caméra par rapport au joueur (ex: vue studio)")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2f, -4f);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        ApplySpawnPoint();
        ActivateAndPositionCamera();
    }

    /// <summary>
    /// Applique le point de spawn du joueur local.
    /// </summary>
    private void ApplySpawnPoint()
    {
        var point = NetworkPlayerManager.Instance?.GetSpawnPoint(OwnerClientId);
        if (point != null)
        {
            transform.SetPositionAndRotation(point.position, point.rotation);
            Debug.Log($"[NetworkPlayer] 🧍 Positionné au point de spawn pour ClientId {OwnerClientId}");
        }
        else
        {
            Debug.LogWarning("[NetworkPlayer] ⚠ Aucun point de spawn trouvé.");
        }
    }


    /// <summary>
    /// Active et positionne la caméra du joueur local dans une vue statique de customisation.
    /// </summary>
    private void ActivateAndPositionCamera()
    {
        if (playerCustomizationCamera == null)
        {
            Debug.LogWarning("[NetworkPlayer] ⚠️ Caméra de customisation non assignée.");
            return;
        }

        playerCustomizationCamera.enabled = true;

        // Position fixe par rapport au joueur à son spawn
        Vector3 cameraPosition = transform.position + cameraOffset;
        playerCustomizationCamera.transform.position = cameraPosition;

        // Regarder légèrement au-dessus du torse
        playerCustomizationCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);

        Debug.Log("[NetworkPlayer] 📷 Caméra activée et positionnée pour le joueur local.");
    }
}
