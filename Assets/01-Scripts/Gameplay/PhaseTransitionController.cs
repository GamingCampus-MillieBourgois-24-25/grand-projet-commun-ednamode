using System.Collections;
using Unity.Netcode;
using CharacterCustomization;

using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Unity.Netcode.Components;

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
        SetPhase(GamePhaseManager.GamePhase.ThemeDisplay);
        ThemeUIManager.Instance.DisplayThemeSequence();
        yield return new WaitForSeconds(5f);

        // ============= Phase d'attente ============= //
        SetPhase(GamePhaseManager.GamePhase.Customization);
        yield return new WaitForSeconds(_phaseManager.CustomizationDuration);

        // ============= Phase de défilé ============= //
        SetPhase(GamePhaseManager.GamePhase.RunwayVoting);
        yield return new WaitForSeconds(1f); // ⏳ Laisse le temps aux clients d’avoir les objets instanciés

        // Attendre que toutes les données de personnalisation soient synchronisées
        yield return StartCoroutine(SyncAllPlayerCustomizations());

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
    /// Synchronise toutes les données de personnalisation avant d'appliquer les visuels.
    /// </summary>
    private IEnumerator SyncAllPlayerCustomizations()
    {
        var players = NetworkManager.Singleton.ConnectedClientsList.Select(c => c.PlayerObject.GetComponent<PlayerCustomizationData>()).ToList();
        Debug.Log($"[GamePhaseTransition] Synchronisation des données pour {players.Count} joueurs.");

        foreach (var player in players)
        {
            // Appeler SyncCustomizationDataServerRpc pour garantir que les données sont à jour
            player.SyncCustomizationDataServerRpc(player.Data);
            Debug.Log($"[GamePhaseTransition] Données envoyées pour joueur {player.OwnerClientId}.");
        }

        // Attendre un court délai pour laisser le temps à la synchronisation réseau
        yield return new WaitForSeconds(0.5f);

        // Vérifier que les données sont bien reçues sur les clients
        foreach (var player in players)
        {
            Debug.Log($"[GamePhaseTransition] Vérification des données pour joueur {player.OwnerClientId}:");
            foreach (var kvp in player.Data.equippedColors)
            {
                Debug.Log($"[GamePhaseTransition] Couleur pour {kvp.Key}: {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
            }
        }
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

        if (phase == GamePhaseManager.GamePhase.ReturnToLobby)
        {
            ResetPlayersPositionAndCamera();
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("Online Panels");
            return;
        }

        var mapping = _phaseManager.GetActivePanelMapping();
        if (mapping == null) return;

        GameObject toHide = null;
        GameObject toShow = null;

        switch (phase)
        {
            case GamePhaseManager.GamePhase.ThemeDisplay:
                toHide = mapping.themeDisplayPanelToHide;
                toShow = mapping.themeDisplayPanel;
                break;
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
        }

        if (toHide != null && toHide.activeSelf)
            UIManager.Instance.HidePanel(toHide);

        if (toShow != null && !toShow.activeSelf)
            UIManager.Instance.ShowPanelDirect(toShow);
    }

    private void ResetPlayersPositionAndCamera()
    {
        var players = FindObjectsOfType<NetworkPlayer>();
        foreach (var player in players)
        {
            var spawnPosition = NetworkPlayerManager.Instance.GetAssignedSpawnPosition(player.OwnerClientId);
            var netTransform = player.GetComponent<NetworkTransform>();
            netTransform.Teleport(spawnPosition, Quaternion.identity, player.transform.localScale);

            var cam = player.GetLocalCamera();
            if (cam != null)
            {
                cam.transform.SetParent(player.transform);
                cam.transform.localPosition = Vector3.zero;
                cam.transform.localRotation = Quaternion.identity;
                Debug.Log($"[Lobby] Caméra réassignée pour le joueur {player.OwnerClientId}");
            }
        }
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

        Debug.Log($"[ApplyAllPlayersVisualsClientRpc] Application des visuels pour {allPlayers.Length} joueurs.");
        foreach (var playerData in allPlayers)
        {
            Debug.Log($"[ApplyAllPlayersVisualsClientRpc] Joueur {playerData.OwnerClientId} données :");
            foreach (var kvp in playerData.Data.equippedColors)
            {
                Debug.Log($"[ApplyAllPlayersVisualsClientRpc] Couleur pour {kvp.Key}: {ColorUtility.ToHtmlStringRGBA(kvp.Value)}");
            }

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
}
#endregion