using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gère les transitions UI pour toutes les phases de jeu, de façon synchrone entre clients et host.
/// Utilise les mappings dynamiques provenant de GamePhaseManager.
/// </summary>
public class GamePhaseTransitionController : NetworkBehaviour
{
    public static GamePhaseTransitionController Instance { get; private set; }

    private GamePhaseManager _phaseManager;
    [SerializeField] private float delayBetweenPhases = 3f;

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
    }

    public void SetPhase(GamePhaseManager.GamePhase newPhase)
    {
        if (!IsServer) return;

        SyncPhaseClientRpc((int)newPhase);
        ApplyPhaseLocally(newPhase);
        _phaseManager.CurrentPhase.Value = newPhase;
    }

    [ClientRpc]
    private void SyncPhaseClientRpc(int phaseValue)
    {
        ApplyPhaseLocally((GamePhaseManager.GamePhase)phaseValue);
    }

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
            case GamePhaseManager.GamePhase.Runway:
                toHide = mapping.runwayPanelToHide;
                toShow = mapping.runwayPanel;
                break;
            case GamePhaseManager.GamePhase.Voting:
                toHide = mapping.votingPanelToHide;
                toShow = mapping.votingPanel;
                break;
            case GamePhaseManager.GamePhase.Podium:
                toHide = mapping.votingPanelToHide; // depuis vote vers podium
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

    /// <summary>
    /// Enchaîne automatiquement toutes les phases du jeu dans l'ordre.
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

        SetPhase(GamePhaseManager.GamePhase.Runway);
        yield return new WaitForSeconds(delayBetweenPhases);

        SetPhase(GamePhaseManager.GamePhase.Voting);
        yield return new WaitForSeconds(delayBetweenPhases);

        SetPhase(GamePhaseManager.GamePhase.Podium);
        yield return new WaitForSeconds(delayBetweenPhases);

        SetPhase(GamePhaseManager.GamePhase.ReturnToLobby);
    }
}