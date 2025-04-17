// ?? RunwayManager : Orchestration des défilés joueur par joueur
// Gère le cycle du défilé, déclenche l'UI (RunwayUIManager), les votes, le timing, la caméra, etc.

using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RunwayManager : NetworkBehaviour
{
    #region ?? Références

    public static RunwayManager Instance { get; private set; }

    [Header("🎥 Défilé")]
    [Tooltip("Durée d'un passage de défilé par joueur (vote inclus)")]
    [SerializeField] private float runwayDurationPerPlayer = 7f;

    [Tooltip("Caméra principale utilisée pour suivre le défilé")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Offsets et paramètres de focus caméra")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);

    [Header("Effets")]
    [Tooltip("SFX à jouer pour annoncer un joueur")]
    [SerializeField] private AudioClip runwayAnnounceSFX;

    [Tooltip("AudioSource utilisée pour jouer les effets sonores")]
    [SerializeField] private AudioSource sfxAudioSource;

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
        FocusCameraOn(clientId);
        PlayIntroSFX();
    }

    [ClientRpc]
    private void EndRunwayForClientRpc(ulong clientId)
    {
        if (!IsClient) return;
        RunwayUIManager.Instance?.HideRunwayPanel();
    }

    #endregion

    #region ?? Caméra & SFX

    private void FocusCameraOn(ulong clientId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (player == null || mainCamera == null) return;

        mainCamera.transform.position = player.transform.position + cameraOffset;
        mainCamera.transform.LookAt(player.transform);
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
