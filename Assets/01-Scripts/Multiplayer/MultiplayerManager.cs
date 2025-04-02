// MULTIPLAYER MANAGER � Stable Unity 6 (Lobby + Relay + AuthGuard)
// G�re : Authentification unique, cr�ation/join/lobby, netcode

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 8;
    [SerializeField] private float lobbyHeartbeatInterval = 15f;

    public Lobby CurrentLobby { get; private set; }
    public string JoinCode { get; private set; }

    private float heartbeatTimer;
    private bool isReady;

    public bool IsReady { get; private set; } = false;

    private async void Awake()
    {
        // S'assurer qu'un seul existe
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsReady = false;
    }

    private async void Start()
    {
        isReady = false;
        await AuthGuard.EnsureSignedInAsync();
        isReady = AuthenticationService.Instance.IsSignedIn;
        Debug.Log($"MultiplayerManager isReady = {isReady}");
    }


    private void Update()
    {
        if (CurrentLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = lobbyHeartbeatInterval;
                SendHeartbeat();
            }
        }
    }

    private async void SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Lobby] Heartbeat failed: " + e.Message);
        }
    }

    public async void CreateLobby(string lobbyName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthGuard.EnsureSignedInAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Impossible de cr�er un lobby sans authentification.");
            return;
        }

        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore pr�t.");
            return;
        }

        try
        {
            var (allocation, joinCode) = await RelayUtils.CreateRelayAsync(maxPlayers);

            if (allocation == null || string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("�chec allocation Relay: allocation ou code null.");
                return;
            }

            JoinCode = joinCode;

            var playerName = "Joueur_" + UnityEngine.Random.Range(1000, 9999);

            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            };

            var createOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player(id: AuthenticationService.Instance.PlayerId, data: playerData),
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode) }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);

            if (CurrentLobby == null)
            {
                Debug.LogError("�chec cr�ation lobby: Lobby est null apr�s CreateLobbyAsync");
                return;
            }

            SessionStore.Instance.SetLobby(CurrentLobby);
            RelayUtils.StartHost(allocation);

            Debug.Log("Lobby cr��: " + lobbyName + " | Code: " + JoinCode);
        }
        catch (Exception e)
        {
            Debug.LogError("�chec cr�ation lobby: " + e.Message);
        }
    }

    public async void JoinLobbyByCode(string code)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore pr�t.");
            return;
        }

        try
        {
            var playerName = "Joueur_" + UnityEngine.Random.Range(1000, 9999);

            var joinOptions = new JoinLobbyByCodeOptions
            {
                Player = new Player(id: AuthenticationService.Instance.PlayerId, data: new Dictionary<string, PlayerDataObject>
                {
                    { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                })
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinOptions);
            if (CurrentLobby == null)
            {
                Debug.LogError("JoinLobbyByCodeAsync a retourn� null.");
                return;
            }

            SessionStore.Instance.SetLobby(CurrentLobby);

            string relayCode = CurrentLobby.Data?["joinCode"]?.Value;
            if (string.IsNullOrEmpty(relayCode))
            {
                Debug.LogError("JoinCode introuvable dans les donn�es du lobby.");
                return;
            }

            var allocation = await RelayUtils.JoinRelayAsync(relayCode);
            RelayUtils.StartClient(allocation);

            Debug.Log("Rejoint lobby avec code: " + code);
        }
        catch (Exception e)
        {
            Debug.LogError("�chec join lobby: " + e.Message);
        }
    }

    public async void QuickJoin()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore pr�t.");
            return;
        }

        try
        {
            CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            SessionStore.Instance.SetLobby(CurrentLobby);

            string relayCode = CurrentLobby.Data?["joinCode"]?.Value;
            if (string.IsNullOrEmpty(relayCode))
            {
                Debug.LogError("JoinCode manquant dans QuickJoin.");
                return;
            }

            var allocation = await RelayUtils.JoinRelayAsync(relayCode);
            RelayUtils.StartClient(allocation);

            Debug.Log("Quick Join lobby: " + CurrentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                Debug.LogWarning("Aucun lobby trouv�, cr�ation automatique.");
                CreateLobby("LobbyAuto");
            }
            else Debug.LogError("Quick Join fail: " + e.Message);
        }
    }

    public async void LeaveLobby()
    {
        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore pr�t.");
            return;
        }

        try
        {
            if (CurrentLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("LeaveLobby: " + e.Message);
        }
        finally
        {
            CurrentLobby = null;
            JoinCode = null;
            SessionStore.Instance.SetLobby(null);
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Session quitt�e.");
        }
    }
}
