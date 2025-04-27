// SESSION STORE – Centralise les infos multijoueur (Lobby, JoinCode)
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;

public class SessionStore : MonoBehaviour
{
    public static SessionStore Instance { get; private set; }

    [Header("Session Info")]
    public Lobby CurrentLobby { get; private set; }
    public string JoinCode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetLobby(Lobby lobby)
    {
        CurrentLobby = lobby;
        JoinCode = lobby?.Data != null && lobby.Data.ContainsKey("joinCode")
            ? lobby.Data["joinCode"].Value
            : null;
    }

    public void Clear()
    {
        CurrentLobby = null;
        JoinCode = null;
    }

    public Dictionary<ulong, string> clientIdToPlayerId = new();
    public Dictionary<string, ulong> playerIdToClientId = new();

    public void RegisterClient(ulong clientId, string playerId)
    {
        if (!clientIdToPlayerId.ContainsKey(clientId))
            clientIdToPlayerId.Add(clientId, playerId);

        if (!playerIdToClientId.ContainsKey(playerId))
            playerIdToClientId.Add(playerId, clientId);
    }

    public string GetPlayerId(ulong clientId)
    {
        return clientIdToPlayerId.TryGetValue(clientId, out var id) ? id : null;
    }

    public ulong GetClientId(string playerId)
    {
        return playerIdToClientId.TryGetValue(playerId, out var cid) ? cid : ulong.MaxValue;
    }

    public string GetPlayerName(string playerId)
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning($"[SessionStore] GetPlayerName() appelé alors que CurrentLobby est null");
            return $"(offline) {playerId}";
        }

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("[SessionStore] playerId est vide ou null");
            return "(invalid id)";
        }

        foreach (var player in CurrentLobby.Players)
        {
            if (player.Id == playerId && player.Data != null &&
                player.Data.TryGetValue("name", out var data) && !string.IsNullOrEmpty(data.Value))
            {
                return data.Value;
            }
        }

        return $"Player_{playerId}";
    }

    public string GetLocalPlayerId()
    {
        return AuthenticationService.Instance?.PlayerId;
    }

    public async Task RefreshLobbyAsync(Action<Lobby> onUpdated = null)
    {
        if (string.IsNullOrEmpty(CurrentLobby?.Id))
        {
            Debug.LogWarning("[SessionStore] ⚠️ Impossible de rafraîchir le lobby : ID manquant");
            return;
        }

        try
        {
            Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            SetLobby(updatedLobby);
            Debug.Log("[SessionStore] ✅ Lobby mis à jour depuis le serveur.");

            onUpdated?.Invoke(updatedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"[SessionStore] ❌ Erreur lors du refresh lobby : {e.Message}");
        }
    }

}
