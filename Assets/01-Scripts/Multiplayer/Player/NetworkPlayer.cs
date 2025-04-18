using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Netcode.Components;

/// <summary>
/// Préfab réseau du joueur. Chaque joueur est téléporté à son point de spawn et possède sa propre caméra locale instanciée dynamiquement.
/// Cette caméra ne concerne que le joueur propriétaire et ne provoque aucun conflit entre clients.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer : NetworkBehaviour
{
    [Header("🎯 Décalage de la caméra locale (vue customisation)")]
    [Tooltip("Position relative de la caméra par rapport au joueur.")]
    [SerializeField] private Vector3 cameraOffset = new(0f, 2f, -4f);

    private CharacterController controller;
    private Camera localCamera;

    public Camera GetLocalCamera() => localCamera;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            Debug.Log("🎮 Je suis le joueur local !");
            CreateAndAttachLocalCamera();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TeleportToSpawnPoint();
        }

        if (IsOwner)
        {
            CreateAndAttachLocalCamera();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkPlayerManager.Instance?.ReleaseSpawnPoint(OwnerClientId);

            // 🔥 Nettoyage des objets orphelins RootBody
            foreach (var root in GameObject.FindObjectsOfType<Transform>())
            {
                if (root.name == "RootBody" && root.parent == null)
                {
                    Destroy(root.gameObject);
                    Debug.Log("[NetworkPlayer] 🧹 RootBody orphelin détruit");
                }
            }
        }
    }

    /// <summary>
    /// Téléporte le joueur à son point de spawn défini.
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
    /// Instancie une caméra propre à ce joueur local uniquement.
    /// </summary>
    private void CreateAndAttachLocalCamera()
    {
        GameObject camObj = new GameObject($"LocalCamera_{OwnerClientId}");
        localCamera = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();

        localCamera.clearFlags = CameraClearFlags.Skybox;
        localCamera.fieldOfView = 60f;
        localCamera.nearClipPlane = 0.1f;
        localCamera.farClipPlane = 100f;

        camObj.transform.SetParent(transform);
        camObj.transform.localPosition = cameraOffset;

        Debug.Log("[NetworkPlayer] 🎥 Caméra locale créée pour ce joueur");
    }
}
