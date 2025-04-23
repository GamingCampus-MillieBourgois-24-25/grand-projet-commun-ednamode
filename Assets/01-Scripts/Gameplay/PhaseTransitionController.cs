using System.Collections;
using Unity.Netcode;
using CharacterCustomization;

using UnityEngine;
using System.Linq;

/// <summary>
/// Contrôleur des transitions synchronisées entre les phases du jeu (host -> clients).
/// </summary>
public class GamePhaseTransitionController : NetworkBehaviour
{
    public static GamePhaseTransitionController Instance { get; private set; }

    [Tooltip("Délai entre chaque grande phase (en secondes)")]
    [SerializeField] private float delayBetweenPhases = 3f;

    [Tooltip("Référence vers le RunwayManager pour déclencher le défilé")]
    [SerializeField] private RunwayManager runwayManager;

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
        Debug.Log("[GamePhaseTransition] stqrt déclenché");

        _phaseManager = FindObjectOfType<GamePhaseManager>();
        if (_phaseManager == null)
            Debug.LogError("[GamePhaseTransition] Aucun GamePhaseManager trouvé dans la scène.");
        runwayManager = FindObjectOfType<RunwayManager>(); // Ajout
        if (runwayManager == null)
            Debug.LogError("[GamePhaseTransition] Aucun RunwayManager trouvé dans la scène.");
    }
    /// <summary>
    /// Débute la séquence de phases de jeu. S'exécute côté serveur uniquement.
    /// </summary>
    public void StartPhaseSequence()
    {
        if (!IsServer) return;
        StartCoroutine(PhaseSequenceCoroutine());
    }

    private IEnumerator PhaseSequenceCoroutine()
    {
        SetPhase(GamePhaseManager.GamePhase.Customization);
        yield return new WaitForSeconds(_phaseManager.CustomizationDuration);

        SetPhase(GamePhaseManager.GamePhase.RunwayVoting);
        Debug.Log("[GamePhaseTransition] couroutine ok");

        // 🔁 Appliquer les visuels pour tous les joueurs avant les votes/défilés
        ApplyAllPlayersVisualsClientRpc();
        yield return new WaitForSeconds(1f);
        // Déclencher la phase de défilé
        if (runwayManager != null)
        {
            runwayManager.StartRunwayPhase();
            Debug.Log("[GamePhaseTransition] StartRunwayPhase déclenché");
        }
        else
        {
            Debug.LogError("[GamePhaseTransition] RunwayManager est null, impossible de déclencher StartRunwayPhase");
        }
        var players = NetworkManager.Singleton.ConnectedClientsList.Select(c => c.PlayerObject.GetComponent<PlayerCustomizationData>()).ToList();
        foreach (var player in players)
        {
            ShowRunwayForClientRpc(player.OwnerClientId);
            yield return new WaitForSeconds(_phaseManager.RunwayVotingPerPlayerDuration);
        }

        yield return new WaitForSeconds(delayBetweenPhases);

        SetPhase(GamePhaseManager.GamePhase.Podium);
        yield return new WaitForSeconds(_phaseManager.PodiumDuration);

        SetPhase(GamePhaseManager.GamePhase.ReturnToLobby);
    }

    private void SetPhase(GamePhaseManager.GamePhase newPhase)
    {
        if (!IsServer) return;
        SyncPhaseClientRpc((int)newPhase);
        _phaseManager.CurrentPhase.Value = newPhase;
    }

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

    [ClientRpc]
    private void ShowRunwayForClientRpc(ulong playerClientId)
    {
        RunwayUIManager.Instance?.ShowCurrentRunwayPlayer(playerClientId);
    }
}