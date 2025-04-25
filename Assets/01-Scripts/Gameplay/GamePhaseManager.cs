using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using CharacterCustomization;

/// <summary>
/// Mapping personnalisé des panels à afficher/masquer en fonction du mode de jeu.
/// </summary>
[System.Serializable]
public class GameModePanelMapping
{
    [Tooltip("Nom visible uniquement dans l'inspecteur pour identifier le mode.")]
    public string modeName;

    [Header("🕹️ Phase : Categorie/Theme")]
    [Tooltip("Panel à afficher pour la phase de sélection de catégorie et thème.")]
    public GameObject themeDisplayPanel;
    [Tooltip("Panel à masquer en quittant la phase précédente vers la sélection de catégorie et thème.")]
    public GameObject themeDisplayPanelToHide;

    [Header("🎨 Phase : Customisation")]
    [Tooltip("Panel à afficher pour la phase de customisation.")]
    public GameObject customizationPanel;
    [Tooltip("Panel à masquer en quittant la phase précédente vers la customisation.")]
    public GameObject customizationPanelToHide;

    [Header("🕺 Phase : Défilé/Vote")]
    [Tooltip("Panel à afficher pour la phase de défilé/vote.")]
    public GameObject runwayPanel;
    [Tooltip("Panel à masquer en quittant la phase précédente vers le défilé/vote.")]
    public GameObject runwayPanelToHide;

    [Header("🏆 Phase : Podium")]
    [Tooltip("Panel à afficher pour la phase de podium.")]
    public GameObject podiumPanel;
    [Tooltip("Panel à masquer en quittant la phase précédente vers le podium.")]
    public GameObject podiumPanelToHide;

    [Header("🏁 Phase : Retour au lobby")]
    [Tooltip("Panel à afficher pour la phase de retour au lobby.")]
    public GameObject returnToLobbyPanel;
    [Tooltip("Panel à masquer en quittant la phase précédente vers le retour au lobby.")]
    public GameObject returnToLobbyPanelToHide;
}

/// <summary>
/// Gère les différentes phases du jeu : customisation, défilé, vote, podium.
/// Les transitions UI sont configurables dynamiquement par mode de jeu.
/// </summary>
public class GamePhaseManager : NetworkBehaviour
{
    #region 🔗 Références & Phase

    public static GamePhaseManager Instance { get; private set; }

    /// <summary> 
    /// Phase actuelle du jeu.
    /// </summary>
    public enum GamePhase { Waiting, ThemeDisplay, Customization, RunwayVoting, Podium, ReturnToLobby }

    [Tooltip("Phase du jeu en cours.")]
    public NetworkVariable<GamePhase> CurrentPhase = new(writePerm: NetworkVariableWritePermission.Server);

    [Header("⏱️ Durées des phases")]
    [Tooltip("Durée de la phase de customisation avant le défilé.")]
    [SerializeField] private float customizationDuration = 60f;
    public float CustomizationDuration => customizationDuration;

    [Tooltip("Durée d'un passage de défilé (vote inclus) par joueur.")]
    [SerializeField] private float runwayVotingPerPlayerDuration = 7f;
    public float RunwayVotingPerPlayerDuration => runwayVotingPerPlayerDuration;

    [Tooltip("Durée d'affichage du podium.")]
    [SerializeField] private float podiumDuration = 10f;
    public float PodiumDuration => podiumDuration;

    [Tooltip("Référence vers le gestionnaire de transitions synchronisées.")]
    [SerializeField] private GamePhaseTransitionController transitionController;

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
    public GameModePanelMapping GetActivePanelMapping()
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

}
