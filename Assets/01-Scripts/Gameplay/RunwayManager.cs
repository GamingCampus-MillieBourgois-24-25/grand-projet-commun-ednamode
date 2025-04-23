using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Netcode.Components;

public class RunwayManager : NetworkBehaviour
{
    #region Références

    public static RunwayManager Instance { get; private set; }

    [Header("🎥 Défilé")]
    [Tooltip("Durée d'un passage de défilé par joueur (vote inclus)")]
    [SerializeField] private float runwayDurationPerPlayer = 7f;

    [Tooltip("Offsets et paramètres de focus caméra")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);

    [Header("Points de défilé")]
    [Tooltip("Point B - Premier point de déplacement")]
    [SerializeField] private Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f);
    [Tooltip("Point C - Deuxième point de déplacement")]
    [SerializeField] private Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f);

    [Header("Paramètres de défilé")]
    [Tooltip("Durée de la pause au point B (en secondes)")]
    [SerializeField] private float pauseDurationAtB = 2f;

    [Header("Effets")]
    [Tooltip("SFX à jouer pour annoncer un joueur")]
    [SerializeField] private AudioClip runwayAnnounceSFX;

    [Tooltip("AudioSource utilisée pour jouer les effets sonores")]
    [SerializeField] private AudioSource sfxAudioSource;


    [Header("Référence au contrôleur de défilé")]
    [Tooltip("Référence au CharacterParadeController pour gérer le déplacement A-B-C-D")]
    [SerializeField] private CharacterParadeController paradeController;

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

    public void StartRunwayPhase()
    {
        if (!IsServer) return;

        Debug.Log("[Runway] 🚀 Début de la phase de défilé !");

        orderedPlayers = NetworkManager.Singleton.ConnectedClientsList
            .Select(c => c.ClientId)
            .OrderBy(id => id)
            .ToList();
        Debug.Log($"[RunwayManager] Joueurs connectés : {string.Join(", ", orderedPlayers)}");

        if (orderedPlayers.Count == 0)
        {
            Debug.LogWarning("[Runway] Aucun joueur connecté, défilé annulé.");
            return;
        }

        StartCoroutine(RunwaySequenceCoroutine());
    }

    private IEnumerator RunwaySequenceCoroutine()
    {
        foreach (var clientId in orderedPlayers)
        {
            AskClientToTeleport(clientId); // Téléportation à RunwaySpot (point A)
            StartRunwayForClientRpc(clientId); // UI + caméra locale
            StartParadeMovementClientRpc(clientId); // Déclencher le défilé côté client
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

        var networkPlayer = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (networkPlayer != null)
        {
            var paradeController = networkPlayer.GetComponent<CharacterParadeController>();
            if (paradeController != null)
            {
                paradeController.StartParadeClientRpc();
                Debug.Log($"[RunwayManager] Défilé déclenché pour client {clientId}");
            }
            else
            {
                Debug.LogError($"[RunwayManager] CharacterParadeController non trouvé sur joueur {clientId}");
            }
        }
        else
        {
            Debug.LogError($"[RunwayManager] NetworkPlayer non trouvé pour client {clientId}");
        }

        PlayIntroSFX();
    }

    [ClientRpc]
    private void EndRunwayForClientRpc(ulong clientId)
    {
        if (!IsClient) return;

        RunwayUIManager.Instance?.HideRunwayPanel();

        var networkPlayer = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(clientId);
        if (networkPlayer != null)
        {
            var paradeController = networkPlayer.GetComponent<CharacterParadeController>();
            if (paradeController != null)
            {
                paradeController.StopParadeClientRpc();
                Debug.Log($"[RunwayManager] Défilé arrêté pour client {clientId}");
            }
        }
    }
    #endregion

    #region Défilé et déplacement

    [ClientRpc]
    private void StartParadeMovementClientRpc(ulong clientId)
    {
        // Exécuter uniquement pour le client concerné
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        Debug.Log($"[Runway] Joueur {clientId} : Début du défilé côté client.");
        StartCoroutine(MovePlayerThroughPoints(clientId));
    }

    private IEnumerator MovePlayerThroughPoints(ulong clientId)
    {
        var player = NetworkPlayerManager.Instance.GetLocalPlayer();
        if (player == null)
        {
            Debug.LogWarning($"[Runway] ❌ Joueur local (clientId {clientId}) introuvable pour le défilé.");
            yield break;
        }

        var netTransform = player.GetComponent<NetworkTransform>();
        if (netTransform == null)
        {
            Debug.LogError($"[Runway] ❌ NetworkTransform non trouvé sur le joueur {clientId}.");
            yield break;
        }

        var animator = player.GetComponent<Animator>();
        var networkAnimator = player.GetComponent<NetworkAnimator>();
        if (animator == null)
        {
            Debug.LogWarning($"[Runway] ⚠️ Animator non trouvé sur le joueur {clientId}. Les animations de marche ne seront pas jouées.");
        }
        else if (networkAnimator == null)
        {
            Debug.LogWarning($"[Runway] ⚠️ NetworkAnimator non trouvé sur le joueur {clientId}. Les animations ne seront pas synchronisées en réseau.");
        }
        else
        {
            Debug.Log($"[Runway] Animator et NetworkAnimator trouvés sur le joueur {clientId}. Vérification du paramètre IsWalking...");
        }

        Transform runwaySpot = GameObject.Find("RunwaySpot")?.transform;
        if (runwaySpot == null)
        {
            Debug.LogError("[Runway] 🚫 Aucun RunwaySpot trouvé ! Impossible de démarrer le défilé.");
            yield break;
        }

        Vector3 pointA = runwaySpot.position;

        float movementDuration = (runwayDurationPerPlayer - pauseDurationAtB) / 2f;
        if (movementDuration <= 0)
        {
            Debug.LogWarning($"[Runway] ⚠️ La durée de pause ({pauseDurationAtB}s) est trop longue pour runwayDurationPerPlayer ({runwayDurationPerPlayer}s). Ajustez les valeurs.");
            movementDuration = 1f;
        }

        // Étape 1 : Téléportation au point A (RunwaySpot)
        netTransform.Teleport(pointA, runwaySpot.rotation, player.transform.localScale);
        Debug.Log($"[Runway] Joueur {clientId} téléporté au point A (RunwaySpot) : {pointA}");

        // Étape 2 : Déplacement de A à B avec animation de marche
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
            Debug.Log($"[Runway] Joueur {clientId} : IsWalking défini à true. État actuel :" );
        }
        yield return StartCoroutine(MovePlayerToPosition(clientId, netTransform, pointA, pointB, movementDuration));
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            Debug.Log($"[Runway] Joueur {clientId} : IsWalking défini à false. État actuel :");
        }
        Debug.Log($"[Runway] Joueur {clientId} arrivé au point B : {pointB}");

        // Étape 3 : Pause au point B
        Debug.Log($"[Runway] Joueur {clientId} en pause au point B pendant {pauseDurationAtB} secondes.");
        yield return new WaitForSeconds(pauseDurationAtB);

        // Étape 4 : Déplacement de B à C avec animation de marche
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
            Debug.Log($"[Runway] Joueur {clientId} : IsWalking défini à true. État actuel ");
        }
        yield return StartCoroutine(MovePlayerToPosition(clientId, netTransform, pointB, pointC, movementDuration));
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            Debug.Log($"[Runway] Joueur {clientId} : IsWalking défini à false. État actuel : ");
        }
        Debug.Log($"[Runway] Joueur {clientId} arrivé au point C : {pointC}");
    }

    private IEnumerator MovePlayerToPosition(ulong clientId, NetworkTransform netTransform, Vector3 startPos, Vector3 targetPos, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector3 newPosition = Vector3.Lerp(startPos, targetPos, t);

            // Synchroniser la position via NetworkTransform
            netTransform.Teleport(newPosition, Quaternion.identity, netTransform.transform.localScale);

            yield return null;
        }

        // S'assurer que la position finale est exacte
        netTransform.Teleport(targetPos, Quaternion.identity, netTransform.transform.localScale);
    }

    #endregion

    #region Téléportation

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

        Debug.Log($"[Runway] 🚶 Joueur {clientId} téléporté au RunwaySpot !");
    }

    [ClientRpc]
    private void TeleportClientRpc(ulong targetClientId, ulong executingClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != executingClientId)
            return;

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

        Debug.Log($"[Runway] ✅ Joueur local téléporté au RunwaySpot pour défiler !");
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