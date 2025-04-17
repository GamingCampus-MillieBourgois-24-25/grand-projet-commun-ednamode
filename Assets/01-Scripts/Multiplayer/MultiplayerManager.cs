using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

using LobbyPlayer = Unity.Services.Lobbies.Models.Player;
using Type = NotificationData.NotificationType;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 8;
    [SerializeField] private float lobbyHeartbeatInterval = 15f;

    public Lobby CurrentLobby { get; private set; }
    public string JoinCode { get; private set; }

    private NetworkObject netObj;
    private float heartbeatTimer;
    private bool isReady;

    public bool IsReady { get; private set; } = false;

    private SceneManager sceneManager;

    private async void Awake()
    {
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
        sceneManager = SceneManager.Instance;
        isReady = false;
        await AuthGuard.EnsureSignedInAsync();
        isReady = AuthenticationService.Instance.IsSignedIn;

        Debug.Log($"MultiplayerManager isReady = {isReady}");

        // 🔍 Référence au NetworkObject
        netObj = GetComponent<NetworkObject>();

        // 🔄 Attente que le NetworkManager soit initialisé
        while (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("🕓 Attente du NetworkManager...");
            await Task.Delay(100); // ⏱️ petite pause pour éviter un while infini en frame
        }


        // ⏳ Attendre qu'on soit bien serveur et que tout soit initialisé
        await Task.Delay(500);

        // ✅ Spawn du MultiplayerNetwork
        var net = FindAnyObjectByType<MultiplayerNetwork>();
        // ✅ On spawn si nécessaire
        if (netObj != null && !netObj.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            netObj.Spawn();
            Debug.Log("✅ MultiplayerManager spawné.");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        await SessionStore.Instance.RefreshLobbyAsync(updated =>
        {
            // Action après mise à jour réussie
            FindFirstObjectByType<PlayerListUI>()?.RefreshPlayerList();
        });
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

    #region NETWORKING
    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        string playerId = AuthenticationService.Instance?.PlayerId;
        SessionStore.Instance?.RegisterClient(clientId, playerId);

        playerReadyStates[clientId] = false;

        if (CanWriteNetworkData())
            MultiplayerNetwork.Instance.PlayerCount.Value = playerReadyStates.Count;

        // 🔁 Synchronise l'état pour ce nouveau client
        NotifyReadyCountClientRpc(GetReadyCount(), playerReadyStates.Count);

        UpdateReadyUI();
        _ = SessionStore.Instance.RefreshLobbyAsync(updated =>
        {
            PlayerListUI playerList = FindObjectOfType<PlayerListUI>();
            playerList?.RefreshPlayerList();
        });

        FindFirstObjectByType<MultiplayerUI>()?.OnClientConnected();
        FindAnyObjectByType<PlayerListUI>()?.RefreshPlayerList();

        if (IsHost())
        {
            RefreshLobbyClientRpc();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        string playerId = SessionStore.Instance.GetPlayerId(clientId);
        FindFirstObjectByType<PlayerListUI>()?.AnimatePlayerLeave(playerId);

        // Retirer le joueur de la liste
        if (playerReadyStates.ContainsKey(clientId))
            playerReadyStates.Remove(clientId);

        if (CanWriteNetworkData())
            MultiplayerNetwork.Instance.PlayerCount.Value = playerReadyStates.Count;

        UpdateReadyUI();
        _ = SessionStore.Instance.RefreshLobbyAsync(updated =>
        {
            FindAnyObjectByType<PlayerListUI>()?.RefreshPlayerList();
            UIManager.Instance?.CancelCountdown();
        });

        if (IsHost())
        {
            RefreshLobbyClientRpc();
        }

    }

    [ClientRpc]
    private void RefreshLobbyClientRpc()
    {
        _ = SessionStore.Instance.RefreshLobbyAsync(updated =>
        {
            FindFirstObjectByType<PlayerListUI>()?.RefreshPlayerList();
        });
    }


    [ClientRpc]
    private void NotifyReadyCountClientRpc(int ready, int total)
    {
        FindFirstObjectByType<MultiplayerUI>()?.UpdateReadyCount(ready, total);
    }

    [ClientRpc]
    private void UpdateReadyVisualClientRpc(string targetPlayerId, bool isReady)
    {
        FindFirstObjectByType<PlayerListUI>()?.MarkPlayerReady(targetPlayerId, isReady);
    }
    #endregion

    public bool IsPlayerReady(string playerId)
    {
        foreach (var kvp in playerReadyStates)
        {
            if (SessionStore.Instance.GetPlayerId(kvp.Key) == playerId)
                return kvp.Value;
        }
        return false;
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

    public async Task RefreshLobbyAsync()
    {
        if (SessionStore.Instance.CurrentLobby == null) return;

        string lobbyId = SessionStore.Instance.CurrentLobby.Id;

        try
        {
            var updated = await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Lobby] Erreur lors de la mise à jour du lobby : {ex.Message}");
        }
    }


    public async void CreateLobby(string lobbyName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthGuard.EnsureSignedInAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Impossible de créer un lobby sans authentification.");
            return;
        }

        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore prêt.");
            return;
        }

        try
        {
            var (allocation, joinCode) = await RelayUtils.CreateRelayAsync(maxPlayers);

            if (allocation == null || string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("Échec allocation Relay: allocation ou code null.");
                NotificationManager.Instance.ShowNotification("Relay allocation failed", Type.Important);
                FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
                return;
            }

            

            var playerName = "Joueur_" + UnityEngine.Random.Range(1000, 9999);

            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            };

            var createOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new LobbyPlayer(id: AuthenticationService.Instance.PlayerId, data: playerData),
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode) }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);

            if (CurrentLobby == null)
            {
                Debug.LogError("Échec création lobby: Lobby est null après CreateLobbyAsync");
                NotificationManager.Instance.ShowNotification("Lobby creation failed", Type.Important);
                FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
                return;
            }

            SessionStore.Instance.SetLobby(CurrentLobby);
            RelayUtils.StartHost(allocation);
            JoinCode = joinCode;
            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode) }
                }
            });

            await Task.Delay(1500); // 1.5s pour laisser le lobby exister réellement sur le service
            Debug.Log("Lobby créé: " + lobbyName + " | Code: " + CurrentLobby.LobbyCode);
        }
        catch (Exception e)
        {
            Debug.LogError("Échec création lobby: " + e.Message);
            NotificationManager.Instance.ShowNotification("Lobby creation failed", Type.Important);
            FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
        }
        FindAnyObjectByType<MultiplayerUI>()?.NotifyCreateResult(true);
        FindFirstObjectByType<MultiplayerUI>()?.UpdateJoinCode(CurrentLobby.LobbyCode);
        Debug.Log($"[LOBBY] Code d'invitation du lobby : {CurrentLobby.LobbyCode}");
        Debug.Log($"[RELAY] Code de Relay : {JoinCode}");

    }

    public async void JoinLobbyByCode(string code, MultiplayerUI multiplayerUI)
    {
        Debug.Log($"🔎 Tentative de rejoindre avec code : {code}");

        try
        {
            var joinOptions = new JoinLobbyByCodeOptions
            {
                Player = new Unity.Services.Lobbies.Models.Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                    { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Client_" + UnityEngine.Random.Range(0, 9999)) }
                    }
                )
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinOptions);
            SessionStore.Instance.SetLobby(CurrentLobby);

            string relayJoinCode = CurrentLobby.Data.ContainsKey("joinCode") ? CurrentLobby.Data["joinCode"].Value : null;

            if (string.IsNullOrEmpty(relayJoinCode))
            {
                Debug.LogError("[JoinLobby] Aucun joinCode de relay trouvé.");
                NotificationManager.Instance.ShowNotification("Lobby Relay manquant", Type.Important);

                FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
                return;
            }

            Debug.Log($"[JoinLobby] Relay join code reçu : {relayJoinCode}");

            var allocation = await RelayUtils.JoinRelayAsync(relayJoinCode);
            RelayUtils.StartClient(allocation);

            FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(true);
            UIManager.Instance?.HideAllPanels();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Échec join lobby: " + e.Message);
            NotificationManager.Instance.ShowNotification("Join Failed", Type.Important);
            FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
        }
    }

    public async void QuickJoin()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore prêt.");
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
                FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
                return;
            }

            var allocation = await RelayUtils.JoinRelayAsync(relayCode);
            RelayUtils.StartClient(allocation);

            Debug.Log("Quick Join lobby: " + CurrentLobby.Name);
            FindFirstObjectByType<MultiplayerUI>()?.UpdateConnectionUI(true);
            FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(true);
            UIManager.Instance?.HideAllPanels();
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                Debug.LogWarning("Aucun lobby trouvé");
                FindAnyObjectByType<MultiplayerUI>()?.NotifyNoLobbyFound();
            }
            else
            {
                Debug.LogError("Quick Join fail: " + e.Message);
                FindAnyObjectByType<MultiplayerUI>()?.NotifyJoinResult(false);
            }
        }
        FindFirstObjectByType<MultiplayerUI>()?.UpdateJoinCode(CurrentLobby.LobbyCode);
    }

    public async void LeaveLobby()
    {
        if (!isReady)
        {
            Debug.LogWarning("MultiplayerManager pas encore prêt.");
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
            Debug.Log("Session quittée.");
        }
        FindFirstObjectByType<MultiplayerUI>()?.UpdateJoinCode("");
    }

    #region GAME START MANAGEMENT

    private Dictionary<ulong, bool> playerReadyStates = new();

    public void SetReady(bool isReady)
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            SubmitReadyServerRpc(isReady);
            PlayerListUI ui = FindFirstObjectByType<PlayerListUI>();
            if (ui != null)
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                ui.MarkPlayerReady(playerId, isReady);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[ServerRpc] Client {clientId} isReady = {isReady}");

        playerReadyStates[clientId] = isReady;
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        string playerId = SessionStore.Instance.GetPlayerId(senderClientId);

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning($"[⚠️ ServerRpc] Impossible de récupérer playerId pour clientId = {senderClientId}. Fallback sur AuthenticationService.");
            playerId = AuthenticationService.Instance.PlayerId;
        }
        UpdatePlayerReadyVisualClientRpc(playerId, isReady);
        UpdateReadyVisualClientRpc(playerId, isReady);
        UpdateReadyCount();
        NotifyReadyCountClientRpc(GetReadyCount(), playerReadyStates.Count);

        if (AllPlayersReady())
            StartCountdown();
        else
        {
            Debug.Log($"[ServerRpc] Pas tous les joueurs prêts. {GetReadyCount()}/{playerReadyStates.Count} prêts.");
            UIManager.Instance?.CancelCountdown();
        }
        UpdateReadyVisualClientRpc(playerId, isReady);

    }

    [ClientRpc]
    private void UpdatePlayerReadyVisualClientRpc(string playerId, bool isReady)
    {
        FindFirstObjectByType<PlayerListUI>()?.MarkPlayerReady(playerId, isReady);
    }


    private void UpdateReadyCount()
    {
        if (!CanWriteNetworkData()) return;

        MultiplayerNetwork.Instance.ReadyCount.Value = GetReadyCount();
        MultiplayerNetwork.Instance.PlayerCount.Value = playerReadyStates.Count;
    }

    public void SelectGameMode(int selectedGameMode)
    {
        if (!IsHost() || !CanWriteNetworkData()) return;

        MultiplayerNetwork.Instance.SelectedGameMode.Value = selectedGameMode;

        if (AllPlayersReady())
            StartCountdown();
    }

    private void StartCountdown()
    {
        StartCountdownClientRpc();
    }

    [ClientRpc]
    private void StartCountdownClientRpc()
    {
        UIManager.Instance?.StartCountdown(() =>
        {
            if (IsHost())
            {
                int selectedGameMode = MultiplayerNetwork.Instance.SelectedGameMode.Value;
                GamePhaseManager.Instance?.StartCustomizationPhase(); // Début du flow logique
                
                /* string sceneToLoad = selectedGameMode switch
                {
                    0 => "Scene_PassMode",
                    1 => "Scene_Sabotage",
                    2 => "Lucie_BasicGameplay",
                    _ => "Mato-Lobby_Horizontal"
                };

                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            */
            }
        });
    }

    private bool AllPlayersReady()
    {
        foreach (var kvp in playerReadyStates)
            if (!kvp.Value) return false;
        return true;
    }

    private int GetReadyCount()
    {
        int count = 0;
        foreach (var r in playerReadyStates.Values)
            if (r) count++;
        return count;
    }


    public void UpdateReadyUI()
    {
        int ready = GetReadyCount();
        int total = playerReadyStates.Count;
        NotifyReadyCountClientRpc(ready, total);
    }

    private new bool IsHost() => NetworkManager.Singleton.IsHost;

    [ClientRpc]
    private void KickClientRpc(ulong clientToKick)
    {
        if (NetworkManager.Singleton.LocalClientId != clientToKick) return;

        Debug.Log("[Kick] Ce client a été expulsé. Retour au menu...");
        StartCoroutine(ReturnToMainMenu());
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return LobbyService.Instance.RemovePlayerAsync(SessionStore.Instance.CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
        NetworkManager.Singleton.Shutdown();
        sceneManager.LoadScene("Lobby_Horizontal v2"); // à adapter si nécessaire
    }


    public void KickPlayerById(string lobbyPlayerId)
    {
        if (!SessionHelper.IsLocalPlayerHost()) return;

        if (SessionHelper.TryGetClientIdFromPlayerId(lobbyPlayerId, out ulong clientId))
        {
            Debug.Log($"[Kick] Expulsion demandée pour {lobbyPlayerId} (clientId {clientId})");
            KickClientRpc(clientId);
        }
    }

    #endregion




    private bool CanWriteNetworkData()
    {
        return MultiplayerNetwork.Instance != null
               && MultiplayerNetwork.Instance.IsSpawned
               && NetworkManager.Singleton.IsServer;
    }

}
