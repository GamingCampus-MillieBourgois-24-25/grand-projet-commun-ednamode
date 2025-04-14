using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;
using CharacterCustomization;
using System.Collections.Generic;

/// <summary>
/// Mapping personnalisé des panels à afficher/masquer en fonction du mode de jeu.
/// </summary>
[System.Serializable]
public class GameModePanelMapping
{
    [Tooltip("Nom visible uniquement dans l'inspecteur pour identifier le mode.")]
    public string modeName;

    [Header("🎨 Phase : Customisation")]
    [Tooltip("Panel à afficher pour la phase de customisation.")]
    public GameObject customizationPanel;
    [Tooltip("Panel à masquer en quittant la phase précédente vers la customisation.")]
    public GameObject customizationPanelToHide;

    [Header("🕺 Phase : Défilé")]
    public GameObject runwayPanel;
    public GameObject runwayPanelToHide;

    [Header("⭐ Phase : Vote")]
    public GameObject votingPanel;
    public GameObject votingPanelToHide;

    [Header("🏆 Phase : Podium")]
    public GameObject podiumPanel;
    public GameObject podiumPanelToHide;
}

/// <summary>
/// Gère les différentes phases du jeu : customisation, défilé, vote, podium.
/// Les transitions UI sont configurables dynamiquement par mode de jeu.
/// </summary>
public class GamePhaseManager : NetworkBehaviour
{
    #region 🔗 Références & Phase

    public static GamePhaseManager Instance { get; private set; }

    /// <summary>Phase actuelle du jeu.</summary>
    public enum GamePhase { Waiting, Customization, Runway, Voting, Podium, ReturnToLobby }

    [Tooltip("Phase du jeu en cours.")]
    public NetworkVariable<GamePhase> CurrentPhase = new(writePerm: NetworkVariableWritePermission.Server);

    [Tooltip("Durée de la phase de customisation avant le défilé.")]
    [SerializeField] private float customizationDuration = 60f;

    [Tooltip("Référence vers le gestionnaire de transitions synchronisées.")]
    [SerializeField] private PhaseTransitionController transitionController;

    [Tooltip("Liste des mappings UI pour chaque mode de jeu.")]
    [SerializeField] private List<GameModePanelMapping> panelMappings;

    #endregion

    #region 🔄 Initialisation

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            CurrentPhase.Value = GamePhase.Waiting;
    }

    #endregion

    #region 🧠 Utilitaires

    /// <summary>
    /// Récupère le mapping de panels pour le mode de jeu actuellement sélectionné.
    /// </summary>
    private GameModePanelMapping GetActivePanelMapping()
    {
        int selected = MultiplayerNetwork.Instance.SelectedGameMode.Value;
        if (selected >= 0 && selected < panelMappings.Count)
            return panelMappings[selected];

        Debug.LogWarning("[GamePhaseManager] Aucune configuration de panel pour ce mode de jeu.");
        return null;
    }

    /// <summary>
    /// Gère la transition UI d'un panel à masquer vers un à afficher.
    /// </summary>
    private void Transition(GameObject toHide, GameObject toShow)
    {
        if (toHide != null && toHide.activeSelf)
            UIManager.Instance.HidePanel(toHide);

        if (toShow != null && !toShow.activeSelf)
            UIManager.Instance.ShowPanelDirect(toShow);
    }


    #endregion

    #region 🎨 PHASES DU JEU

    /// <summary>
    /// Démarre la phase de customisation.
    /// </summary>
    public void StartCustomizationPhase()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Customization;

        var mapping = GetActivePanelMapping();
        if (mapping != null)
            Transition(mapping.customizationPanelToHide, mapping.customizationPanel);

        if (mapping.customizationPanel.TryGetComponent(out CustomisationUIManager ui))
            ui.ForceInit();

        StartCoroutine(CustomizationRoutine());
    }

    /// <summary>
    /// Attente de fin de customisation avant le défilé.
    /// </summary>
    private IEnumerator CustomizationRoutine()
    {
        yield return new WaitForSeconds(customizationDuration);
        StartRunwayPhase();
    }

    /// <summary>
    /// Démarre la phase de défilé synchronisé.
    /// </summary>
    public void StartRunwayPhase()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Runway;

        var mapping = GetActivePanelMapping();
        if (mapping != null)
            Transition(mapping.runwayPanelToHide, mapping.runwayPanel);

        var allItems = Resources.LoadAll<Item>("Items").ToList();
        foreach (var player in FindObjectsOfType<PlayerCustomizationData>())
        {
            var visuals = player.GetComponentInChildren<EquippedVisualsHandler>();
            if (visuals != null)
            {
                visuals.ClearAll();
                player.ApplyToVisuals(visuals, allItems);
            }
        }
    }

    /// <summary>
    /// Démarre la phase de vote.
    /// </summary>
    public void StartVotingPhase()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Voting;

        var mapping = GetActivePanelMapping();
        if (mapping != null)
            Transition(mapping.votingPanelToHide, mapping.votingPanel);
    }

    /// <summary>
    /// Affiche le podium final avec les résultats.
    /// </summary>
    public void ShowPodium()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Podium;

        var mapping = GetActivePanelMapping();
        if (mapping != null)
            Transition(mapping.votingPanelToHide, mapping.podiumPanel);
    }

    /// <summary>
    /// Retourne tous les joueurs dans le lobby.
    /// </summary>
    public void ReturnToLobby()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.ReturnToLobby;

        UIManager.Instance.HideAllPanels();
        UIManager.Instance.ShowPanel("Online");
    }

    #endregion
}