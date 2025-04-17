// ?? RunwayManager : Orchestration des d�fil�s joueur par joueur
// G�re le cycle du d�fil�, d�clenche l'UI (RunwayUIManager), les votes, le timing, la cam�ra, etc.

using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RunwayManager : NetworkBehaviour
{
    #region ?? R�f�rences

    public static RunwayManager Instance { get; private set; }

    [Tooltip("Dur�e d'un passage de d�fil� par joueur (vote inclus)")]
    [SerializeField] private float runwayDurationPerPlayer = 7f;

    [Tooltip("Cam�ra principale utilis�e pour suivre le d�fil�")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Offsets et param�tres de focus cam�ra")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);

    [Tooltip("SFX � jouer pour annoncer un joueur")]
    [SerializeField] private AudioClip runwayAnnounceSFX;

    [Tooltip("AudioSource utilis�e pour jouer les effets sonores")]
    [SerializeField] private AudioSource sfxAudioSource;

    #endregion

    #region ?? Cycle de d�fil�

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
    /// D�bute la s�quence de d�fil� + vote pour tous les joueurs connect�s
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

        // ?? Fin de phase : pr�venir GamePhaseTransitionController ou GameManager si besoin
    }

    #endregion

    #region ?? D�clenchements UI c�t� clients

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

    #region ?? Cam�ra & SFX

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
