using UnityEngine;
using Unity.Netcode;
using System.Collections;

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

        StartCoroutine(DelayedSpawnAndCamera());
    }

    private IEnumerator DelayedSpawnAndCamera()
    {
        // Attends que NetworkPlayerManager.Instance existe et que spawnPoints soient prêts
        yield return new WaitUntil(() => NetworkPlayerManager.Instance != null && NetworkPlayerManager.Instance.spawnPoints.Count > 0);

        ApplySpawnPoint();
        ActivateAndPositionCamera();
    }

    /// <summary>
    /// Applique le point de spawn du joueur local.
    /// </summary>
    private void ApplySpawnPoint()
    {
        var spawnPoint = NetworkPlayerManager.Instance?.GetSpawnPoint(OwnerClientId);
        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"[NetworkPlayer] 🧍 Téléporté au spawn point {spawnPoint.name}");
        }
        else
        {
            Debug.LogWarning("[NetworkPlayer] ⚠ Aucun point de spawn trouvé !");
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
