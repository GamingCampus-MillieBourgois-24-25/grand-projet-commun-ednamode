using System.Collections;
using Unity.Netcode;
using CharacterCustomization;

using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

/// <summary>
/// Contrôleur des transitions synchronisées entre les phases du jeu (host -> clients).
/// </summary>
public class GamePhaseTransitionController : NetworkBehaviour
{
    public static GamePhaseTransitionController Instance { get; private set; }

    [Tooltip("Délai entre chaque grande phase (en secondes)")]
    [SerializeField] private float delayBetweenPhases = 3f;

    [Header("🎥 PostProcess Settings")]
    [SerializeField] private Volume postProcessVolume;

    private DepthOfField depthOfField;
    private float initialFocusDistance;

    private GamePhaseManager _phaseManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _phaseManager = FindObjectOfType<GamePhaseManager>();
        if (_phaseManager == null)
            Debug.LogError("[GamePhaseTransition] Aucun GamePhaseManager trouvé dans la scène.");

        if (postProcessVolume == null)
        {
            Debug.LogError("[GamePhaseTransition] Volume PostProcess non assigné !");
            return;
        }

        if (postProcessVolume.profile.TryGet(out depthOfField))
        {
            initialFocusDistance = depthOfField.focusDistance.value;
            Debug.Log($"[PostProcess] Valeur initiale du DoF enregistrée : {initialFocusDistance}");
        }
        else
        {
            Debug.LogError("[PostProcess] Aucun DepthOfField trouvé dans le profil !");
        }
    }

    #region Phases de jeu

    /// <summary>
    /// Débute la séquence de phases de jeu. S'exécute côté serveur uniquement.
    /// </summary>
    public void StartPhaseSequence()
    {
        if (!IsServer) return;
        StartCoroutine(PhaseSequenceCoroutine());
    }

    /// <summary>
    /// Coroutine de la séquence de phases de jeu.
    /// </summary>
    private IEnumerator PhaseSequenceCoroutine()
    {
        // ============= Phase d'attente ============= //
        SetPhase(GamePhaseManager.GamePhase.Customization);
        yield return new WaitForSeconds(_phaseManager.CustomizationDuration);

        // ============= Phase de défilé ============= //
        SetPhase(GamePhaseManager.GamePhase.RunwayVoting);
        yield return new WaitForSeconds(1f);

        ApplyAllPlayersVisualsClientRpc();
        yield return new WaitForSeconds(1f);
        RunwayManager.Instance.StartRunwayPhase();

        var players = NetworkManager.Singleton.ConnectedClientsList.Select(c => c.PlayerObject.GetComponent<PlayerCustomizationData>()).ToList();
        foreach (var player in players)
        {
            ShowRunwayForClientRpc(player.OwnerClientId);
            yield return new WaitForSeconds(_phaseManager.RunwayVotingPerPlayerDuration);
        }

        yield return new WaitForSeconds(delayBetweenPhases);

        // ============= Phase de podium ============= //
        SetPhase(GamePhaseManager.GamePhase.Podium);
        PodiumManager.Instance.StartPodiumSequence();
        yield return new WaitForSeconds(_phaseManager.PodiumDuration);

        // ============= Retour au lobby ============= //
        SetPhase(GamePhaseManager.GamePhase.ReturnToLobby);
    }

    /// <summary>
    /// Change la phase de jeu et synchronise l'affichage sur tous les clients.
    /// </summary>
    private void SetPhase(GamePhaseManager.GamePhase newPhase)
    {
        if (!IsServer) return;
        SyncPhaseClientRpc((int)newPhase);
        _phaseManager.CurrentPhase.Value = newPhase;
    }

    /// <summary>
    /// Synchronise la phase de jeu sur tous les clients.
    /// </summary>
    [ClientRpc]
    private void SyncPhaseClientRpc(int phaseValue)
    {
        ApplyPhaseLocally((GamePhaseManager.GamePhase)phaseValue);
    }

    /// <summary>
    /// Affiche localement le panel correspondant à la phase.
    /// </summary>
    private void ApplyPhaseLocally(GamePhaseManager.GamePhase phase)
    {
        if (_phaseManager == null) return;

        HandleDepthOfField(phase);

        var mapping = _phaseManager.GetActivePanelMapping();
        if (mapping == null)
        {
            Debug.LogWarning("[GamePhaseTransition] Aucun mapping actif disponible.");
            return;
        }

        GameObject toHide = null;
        GameObject toShow = null;

        switch (phase)
        {
            case GamePhaseManager.GamePhase.Customization:
                toHide = mapping.customizationPanelToHide;
                toShow = mapping.customizationPanel;
                break;
            case GamePhaseManager.GamePhase.RunwayVoting:
                toHide = mapping.runwayPanelToHide;
                toShow = mapping.runwayPanel;
                break;
            case GamePhaseManager.GamePhase.Podium:
                toHide = mapping.podiumPanelToHide;
                toShow = mapping.podiumPanel;
                break;
            case GamePhaseManager.GamePhase.ReturnToLobby:
            case GamePhaseManager.GamePhase.Waiting:
                UIManager.Instance.HideAllPanels();
                UIManager.Instance.ShowPanel("Online Panels");
                return;
        }

        if (toHide != null && toHide.activeSelf)
            UIManager.Instance.HidePanel(toHide);

        if (toShow != null && !toShow.activeSelf)
            UIManager.Instance.ShowPanelDirect(toShow);
    }

    #endregion

    #region UI & Visuels

    /// <summary>
    /// Ajuste la profondeur de champ en fonction de la phase.
    /// </summary>
    private void HandleDepthOfField(GamePhaseManager.GamePhase phase)
    {
        if (depthOfField == null) return;

        switch (phase)
        {
            case GamePhaseManager.GamePhase.Customization:
            case GamePhaseManager.GamePhase.RunwayVoting:
            case GamePhaseManager.GamePhase.Podium:
                depthOfField.focusDistance.value = 5f;
                Debug.Log("[PostProcess] DoF ajusté à 5 pour cette phase.");
                break;

            case GamePhaseManager.GamePhase.ReturnToLobby:
            case GamePhaseManager.GamePhase.Waiting:
                depthOfField.focusDistance.value = initialFocusDistance;
                Debug.Log("[PostProcess] DoF restauré à la valeur initiale.");
                break;
        }
    }

    /// <summary>
    /// Applique les visuels de tous les joueurs sur le client.
    /// </summary>
    [ClientRpc]
    private void ApplyAllPlayersVisualsClientRpc()
    {
        var allItems = Resources.LoadAll<Item>("Items").ToList();
        var allPlayers = FindObjectsOfType<PlayerCustomizationData>();

        foreach (var playerData in allPlayers)
        {
            var visuals = playerData.GetComponentInChildren<EquippedVisualsHandler>(true);
            if (visuals != null)
                playerData.ApplyToVisuals(visuals, allItems);
        }
    }

    /// <summary>
    /// Affiche le défilé pour le joueur spécifié.
    /// </summary>
    [ClientRpc]
    private void ShowRunwayForClientRpc(ulong playerClientId)
    {
        RunwayUIManager.Instance?.ShowCurrentRunwayPlayer(playerClientId);
    }

    #endregion
}