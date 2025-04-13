using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;
using CharacterCustomization;

public class GamePhaseManager : NetworkBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    public enum GamePhase { Waiting, Customization, Runway, Voting, Podium, ReturnToLobby }
    public NetworkVariable<GamePhase> CurrentPhase = new(writePerm: NetworkVariableWritePermission.Server);

    [SerializeField] private float customizationDuration = 60f;
    [SerializeField] private PhaseTransitionController transitionController;

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

    public void StartCustomizationPhase()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Customization;
        StartCoroutine(CustomizationRoutine());
        transitionController?.TransitionToPhase("Online", "Customization");
    }

    private IEnumerator CustomizationRoutine()
    {
        yield return new WaitForSeconds(customizationDuration);
        StartRunwayPhase();
    }

    public void StartRunwayPhase()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Runway;
        transitionController?.TransitionToPhase("Customization", "Runway");

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

    public void StartVotingPhase()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Voting;
        transitionController?.TransitionToPhase("Runway", "Voting");
    }

    public void ShowPodium()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.Podium;
        transitionController?.TransitionToPhase("Voting", "Podium");
    }

    public void ReturnToLobby()
    {
        if (!IsServer) return;
        CurrentPhase.Value = GamePhase.ReturnToLobby;
        transitionController?.TransitionToPhase("Podium", "Online");
    }
}