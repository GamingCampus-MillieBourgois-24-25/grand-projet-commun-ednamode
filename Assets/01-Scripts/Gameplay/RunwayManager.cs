// ?? RunwayManager : Orchestration des défilés joueur par joueur
// Gère le cycle du défilé, déclenche l'UI (RunwayUIManager), les votes, le timing, la caméra, etc.

using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class RunwayManager : NetworkBehaviour
{
    #region ?? Références

    public static RunwayManager Instance { get; private set; }

    [Header("🎥 Défilé")]
    [Tooltip("Durée d'un passage de défilé par joueur (vote inclus)")]
    [SerializeField] private float runwayDurationPerPlayer = 7f;

    [Tooltip("Offsets et paramètres de focus caméra")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);

    [Header("Effets")]
    [Tooltip("SFX à jouer pour annoncer un joueur")]
    [SerializeField] private AudioClip runwayAnnounceSFX;

    [Tooltip("AudioSource utilisée pour jouer les effets sonores")]
    [SerializeField] private AudioSource sfxAudioSource;


    [Header("Référence au contrôleur de défilé")]
    [Tooltip("Référence au CharacterParadeController pour gérer le déplacement A-B-C-D")]
    [SerializeField] private CharacterParadeController paradeController;

    #endregion

    #region ?? Cycle de défilé

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

        orderedPlayers = NetworkManager.Singleton.ConnectedClientsList
            .Select(c => c.ClientId)
            .OrderBy(id => id)
            .ToList();
        Debug.Log($"[RunwayManager] Joueurs connectés : {string.Join(", ", orderedPlayers)}");

        StartCoroutine(RunwaySequenceCoroutine());
    }

    private IEnumerator RunwaySequenceCoroutine()
    {
        foreach (var clientId in orderedPlayers)
        {
            StartRunwayForClientRpc(clientId);
            yield return new WaitForSeconds(runwayDurationPerPlayer);
            EndRunwayForClientRpc(clientId);
            yield return new WaitForSeconds(0.5f); // Petite pause entre deux passages
        }

        // ?? Fin de phase : prévenir GamePhaseTransitionController ou GameManager si besoin
    }

    #endregion

    #region ?? Déclenchements UI côté clients

    [ClientRpc]
    private void StartRunwayForClientRpc(ulong clientId)
    {
        if (!IsClient) return;

        RunwayUIManager.Instance?.ShowCurrentRunwayPlayer(clientId);
        DeactivateAllOtherCameras();
        FocusCameraOn(clientId);

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

    #region ?? Caméra & SFX

    private void FocusCameraOn(ulong targetClientId)
    {
        var targetPlayer = NetworkPlayerManager.Instance.GetNetworkPlayerFrom(targetClientId);
        if (targetPlayer == null)
        {
            Debug.LogWarning($"[RunwayManager] ❌ Aucun joueur cible trouvé pour {targetClientId}");
            return;
        }

        Transform lookTarget = GameObject.Find("RunwayTarget")?.transform ?? targetPlayer.transform;

        var localPlayer = NetworkPlayerManager.Instance.GetLocalPlayer();
        if (localPlayer == null)
        {
            Debug.LogError("[RunwayManager] ❌ Aucun joueur local trouvé !");
            return;
        }

        Camera cam = localPlayer.GetLocalCamera();
        if (cam == null)
        {
            Debug.LogError("[RunwayManager] ❌ Caméra locale introuvable !");
            return;
        }

        cam.gameObject.SetActive(true); // Force l'activation

        cam.transform.DOMove(lookTarget.position + cameraOffset, 0.5f).SetEase(Ease.InOutSine);
        cam.transform.DOLookAt(lookTarget.position + Vector3.up * 1.5f, 0.5f).SetEase(Ease.InOutSine);

        Debug.Log($"[RunwayManager] 🎥 Caméra LOCALE déplacée pour observer {targetClientId}");
    }

    private void DeactivateAllOtherCameras()
    {
        foreach (var p in FindObjectsOfType<NetworkPlayer>())
        {
            var cam = p.GetComponentInChildren<Camera>(true);
            if (cam != null) cam.gameObject.SetActive(false);
        }
    }


    private void PlayIntroSFX()
    {
        if (runwayAnnounceSFX != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(runwayAnnounceSFX);
        }
    }

    #endregion

    #region ?? Utilitaires

    public float GetRunwayDuration() => runwayDurationPerPlayer;

    #endregion
}
